using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class OverdueReminderService : IOverdueReminderService
    {
        private readonly IBorrowTicketRepository _ticketRepository;
        private readonly IOverdueReminderLogRepository _reminderLogRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<OverdueReminderService> _logger;
        private readonly int _reminderIntervalMinutes;

        public OverdueReminderService(
            IBorrowTicketRepository ticketRepository,
            IOverdueReminderLogRepository reminderLogRepository,
            IUserRepository userRepository,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<OverdueReminderService> logger)
        {
            _ticketRepository = ticketRepository;
            _reminderLogRepository = reminderLogRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _logger = logger;
            _reminderIntervalMinutes = configuration.GetValue<int?>("EmailSettings:OverdueReminderIntervalMinutes") ?? 1440;
            if (_reminderIntervalMinutes <= 0)
            {
                _reminderIntervalMinutes = 1440;
            }
        }

        public async Task<int> SendOverdueRemindersAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            var overdueTickets = _ticketRepository.GetOverdueTickets().ToList();
            if (!overdueTickets.Any())
            {
                return 0;
            }

            var ticketIds = overdueTickets.Select(t => t.Id).ToList();
            var existingLogs = _reminderLogRepository
                .GetLogsByTicketIds(ticketIds)
                .ToDictionary(l => l.BorrowTicketId, l => l);

            int sentCount = 0;
            foreach (var ticket in overdueTickets)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (!ticket.DueAt.HasValue)
                {
                    continue;
                }

                var user = ticket.User ?? _userRepository.GetById(ticket.UserId);
                if (user == null || !user.ReceiveOverdueEmails)
                {
                    continue;
                }

                existingLogs.TryGetValue(ticket.Id, out var log);
                if (log != null && !log.IsResolved && log.NextSendAt > now)
                {
                    continue;
                }

                var daysOverdue = Math.Max(1, (int)Math.Ceiling((now - ticket.DueAt.Value).TotalDays));
                var fineAmount = daysOverdue * ticket.FinePerDay;

                var emailSent = await _emailService.SendOverdueReminderEmail(
                    user.Email,
                    user.Name,
                    ticket.OrgBook?.Title ?? "your borrowed book",
                    ticket.DueAt.Value,
                    daysOverdue,
                    fineAmount,
                    ticket.OrgBook?.Organization?.Name ?? "WildWisdom");

                if (!emailSent)
                {
                    _logger.LogWarning("Failed to send overdue reminder for ticket {TicketId}", ticket.Id);
                    continue;
                }

                var isNewLog = log == null;
                if (isNewLog)
                {
                    log = new OverdueReminderLog
                    {
                        BorrowTicketId = ticket.Id,
                        UserId = ticket.UserId,
                        CreatedBy = "SYSTEM",
                        CreatedTime = now,
                        ReminderCount = 0
                    };
                    existingLogs[ticket.Id] = log;
                }

                log.LastSentAt = now;
                log.NextSendAt = now.AddMinutes(_reminderIntervalMinutes);
                log.LastFineAmount = fineAmount;
                log.ReminderCount += 1;
                log.UpdatedBy = "SYSTEM";
                log.UpdatedTime = now;
                log.IsResolved = false;

                if (isNewLog)
                {
                    _reminderLogRepository.Add(log);
                }
                else
                {
                    _reminderLogRepository.Update(log);
                }
                sentCount++;
            }

            return sentCount;
        }

        public Dictionary<int, OverdueReminderInfoViewModel> GetReminderStatuses(IEnumerable<int> ticketIds)
        {
            if (ticketIds == null)
            {
                return new Dictionary<int, OverdueReminderInfoViewModel>();
            }

            return _reminderLogRepository
                .GetLogsByTicketIds(ticketIds)
                .ToDictionary(
                    log => log.BorrowTicketId,
                    log => new OverdueReminderInfoViewModel
                    {
                        LastSentAt = log.LastSentAt,
                        NextSendAt = log.NextSendAt,
                        ReminderCount = log.ReminderCount,
                        LastFineAmount = log.LastFineAmount,
                        IsResolved = log.IsResolved
                    });
        }
    }
}


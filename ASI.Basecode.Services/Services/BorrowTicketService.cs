using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class BorrowTicketService : IBorrowTicketService
    {
        private readonly IBorrowTicketRepository _ticketRepository;
        private readonly IOrgBooksRepository _booksRepository;
        private readonly IBorrowingPolicyRepository _borrowingPolicyRepository;
        private readonly IFinePolicyRepository _finePolicyRepository;
        private readonly IRenewalPolicyRepository _renewalPolicyRepository;
        private readonly IClaimPolicyRepository _claimPolicyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly IMapper _mapper;

        public BorrowTicketService(
            IBorrowTicketRepository ticketRepository,
            IOrgBooksRepository booksRepository,
            IBorrowingPolicyRepository borrowingPolicyRepository,
            IFinePolicyRepository finePolicyRepository,
            IRenewalPolicyRepository renewalPolicyRepository,
            IClaimPolicyRepository claimPolicyRepository,
            IUserRepository userRepository,
            IAdminRepository adminRepository,
            IMapper mapper)
        {
            _ticketRepository = ticketRepository;
            _booksRepository = booksRepository;
            _borrowingPolicyRepository = borrowingPolicyRepository;
            _finePolicyRepository = finePolicyRepository;
            _renewalPolicyRepository = renewalPolicyRepository;
            _claimPolicyRepository = claimPolicyRepository;
            _userRepository = userRepository;
            _adminRepository = adminRepository;
            _mapper = mapper;
        }

        public BorrowTicketViewModel CreateBorrowRequest(int userId, int bookId)
        {
            var book = _booksRepository.GetById(bookId);
            if (book == null)
            {
                throw new InvalidDataException("Book not found.");
            }

            // Check book availability
            if (book.CopiesAvailable <= 0)
            {
                throw new InvalidOperationException("No copies available for this book.");
            }

            // Get organization policies
            var borrowingPolicy = _borrowingPolicyRepository.GetByOrganization(book.OrganizationId);
            var finePolicy = _finePolicyRepository.GetByOrganization(book.OrganizationId);
            var renewalPolicy = _renewalPolicyRepository.GetByOrganization(book.OrganizationId);
            var claimPolicy = _claimPolicyRepository.GetByOrganization(book.OrganizationId);

            if (borrowingPolicy == null || finePolicy == null || renewalPolicy == null || claimPolicy == null)
            {
                throw new InvalidDataException("Organization policies not found. Please contact administrator.");
            }

            // Validate borrow eligibility
            if (!ValidateBorrowEligibility(userId, book.OrganizationId))
            {
                throw new InvalidOperationException($"You have reached the maximum number of active tickets ({borrowingPolicy.MaxActiveTicketsPerUser}).");
            }

            // Create ticket
            var ticket = new BorrowTicket
            {
                ReferenceCode = GenerateReferenceCode(book.OrganizationId),
                OrganizationId = book.OrganizationId,
                OrgBookId = bookId,
                UserId = userId,
                Status = TicketStatus.NotYetClaimed,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(claimPolicy.ClaimWindowHours),
                
                // Policy snapshot
                BorrowDays = borrowingPolicy.BorrowDaysDefault,
                FinePerDay = finePolicy.FinePerDay,
                MaxRenewals = renewalPolicy.MaxRenewals,
                RenewCount = 0,
                
                AccruedFine = 0,
                IsFinePaid = false,
                
                CreatedBy = System.Environment.UserName,
                UpdatedBy = System.Environment.UserName,
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now
            };

            // Reserve a copy
            _booksRepository.DecrementCopies(bookId);

            _ticketRepository.AddTicket(ticket);

            return GetTicketById(ticket.Id);
        }

        public string GenerateReferenceCode(int orgId)
        {
            // Format: ORG-YYYYMMDD-XXXXX
            var datePart = DateTime.Now.ToString("yyyyMMdd");
            
            // Get count of tickets created today for this organization
            var today = DateTime.Now.Date;
            var todayTickets = _ticketRepository.GetTicketsByOrganization(orgId)
                .Where(t => t.CreatedAt.Date == today)
                .Count();

            var sequentialPart = (todayTickets + 1).ToString("D5");
            
            return $"ORG{orgId:D3}-{datePart}-{sequentialPart}";
        }

        public void ClaimTicket(string referenceCode, int employeeId, string notes)
        {
            var ticket = _ticketRepository.GetTicketByReferenceCode(referenceCode);
            if (ticket == null)
            {
                throw new InvalidDataException("Ticket not found.");
            }

            if (ticket.Status != TicketStatus.NotYetClaimed)
            {
                throw new InvalidOperationException($"Ticket is already {ticket.Status}. Cannot claim.");
            }

            if (ticket.ExpiresAt < DateTime.Now)
            {
                // Auto-expire
                ticket.Status = TicketStatus.Expired;
                ticket.CancelledAt = DateTime.Now;
                ticket.UpdatedTime = DateTime.Now;
                ticket.UpdatedBy = System.Environment.UserName;
                
                // Release the copy
                _booksRepository.IncrementCopies(ticket.OrgBookId);
                
                _ticketRepository.UpdateTicket(ticket);
                throw new InvalidOperationException("Ticket has expired and cannot be claimed.");
            }

            // Claim the ticket
            var now = DateTime.Now;
            ticket.Status = TicketStatus.Claimed;
            ticket.ClaimedAt = now;
            ticket.CheckedOutAt = now;
            ticket.ClaimedByEmployeeId = employeeId;
            ticket.CheckedOutByEmployeeId = employeeId;
            ticket.DueAt = now.AddDays(ticket.BorrowDays);
            ticket.Notes = notes;
            ticket.UpdatedTime = now;
            ticket.UpdatedBy = System.Environment.UserName;

            // Immediately transition to NotYetReturned status
            ticket.Status = TicketStatus.NotYetReturned;

            _ticketRepository.UpdateTicket(ticket);
        }

        public void ReturnBook(string referenceCode, int employeeId)
        {
            var ticket = _ticketRepository.GetTicketByReferenceCode(referenceCode);
            if (ticket == null)
            {
                throw new InvalidDataException("Ticket not found.");
            }

            if (ticket.Status != TicketStatus.NotYetReturned)
            {
                throw new InvalidOperationException($"Ticket status is {ticket.Status}. Cannot return.");
            }

            var now = DateTime.Now;
            
            // Calculate fine if overdue
            if (ticket.DueAt.HasValue && now > ticket.DueAt.Value)
            {
                var daysOverdue = (int)(now - ticket.DueAt.Value).TotalDays;
                ticket.AccruedFine = daysOverdue * ticket.FinePerDay;
            }

            ticket.Status = TicketStatus.Returned;
            ticket.ReturnedAt = now;
            ticket.ReturnedToEmployeeId = employeeId;
            ticket.UpdatedTime = now;
            ticket.UpdatedBy = System.Environment.UserName;

            // Release the copy
            _booksRepository.IncrementCopies(ticket.OrgBookId);

            _ticketRepository.UpdateTicket(ticket);
        }

        public void CancelTicket(int ticketId, string reason)
        {
            var ticket = _ticketRepository.GetById(ticketId);
            if (ticket == null)
            {
                throw new InvalidDataException("Ticket not found.");
            }

            if (ticket.Status != TicketStatus.NotYetClaimed)
            {
                throw new InvalidOperationException("Only unclaimed tickets can be cancelled.");
            }

            ticket.Status = TicketStatus.Cancelled;
            ticket.CancelledAt = DateTime.Now;
            ticket.Notes = reason;
            ticket.UpdatedTime = DateTime.Now;
            ticket.UpdatedBy = System.Environment.UserName;

            // Release the copy
            _booksRepository.IncrementCopies(ticket.OrgBookId);

            _ticketRepository.UpdateTicket(ticket);
        }

        public void ExpireUnclaimedTickets()
        {
            var expiredTickets = _ticketRepository.GetExpiredTickets().ToList();
            
            foreach (var ticket in expiredTickets)
            {
                ticket.Status = TicketStatus.Expired;
                ticket.CancelledAt = DateTime.Now;
                ticket.UpdatedTime = DateTime.Now;
                ticket.UpdatedBy = "SYSTEM";
                
                // Release the copy
                _booksRepository.IncrementCopies(ticket.OrgBookId);
                
                _ticketRepository.UpdateTicket(ticket);
            }
        }

        public decimal CalculateFine(BorrowTicketViewModel ticket)
        {
            if (!ticket.DueAt.HasValue || ticket.ReturnedAt == null)
            {
                return 0;
            }

            var returnDate = ticket.ReturnedAt.Value;
            var dueDate = ticket.DueAt.Value;

            if (returnDate <= dueDate)
            {
                return 0;
            }

            var daysOverdue = (int)(returnDate - dueDate).TotalDays;
            return daysOverdue * ticket.FinePerDay;
        }

        public List<BorrowTicketViewModel> GetUserTickets(int userId, TicketStatus? status = null)
        {
            var query = _ticketRepository.GetTicketsByUser(userId);

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            var tickets = query.ToList();
            return MapTicketsToViewModels(tickets);
        }

        public List<BorrowTicketViewModel> GetOrganizationTickets(int orgId, TicketStatus? status = null)
        {
            var query = _ticketRepository.GetTicketsByOrganization(orgId);

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            var tickets = query.ToList();
            return MapTicketsToViewModels(tickets);
        }

        public BorrowTicketViewModel GetTicketByReferenceCode(string referenceCode)
        {
            var ticket = _ticketRepository.GetTicketByReferenceCode(referenceCode);
            if (ticket == null)
            {
                return null;
            }

            return MapTicketToViewModel(ticket);
        }

        public BorrowTicketViewModel GetTicketById(int id)
        {
            var ticket = _ticketRepository.GetById(id);
            if (ticket == null)
            {
                return null;
            }

            return MapTicketToViewModel(ticket);
        }

        public bool ValidateBorrowEligibility(int userId, int orgId)
        {
            var borrowingPolicy = _borrowingPolicyRepository.GetByOrganization(orgId);
            if (borrowingPolicy == null)
            {
                return false;
            }

            var activeTicketsCount = _ticketRepository.GetActiveTickets(userId)
                .Count(t => t.OrganizationId == orgId);

            return activeTicketsCount < borrowingPolicy.MaxActiveTicketsPerUser;
        }

        public void RenewTicket(int ticketId)
        {
            var ticket = _ticketRepository.GetById(ticketId);
            if (ticket == null)
            {
                throw new InvalidDataException("Ticket not found.");
            }

            if (ticket.Status != TicketStatus.NotYetReturned)
            {
                throw new InvalidOperationException("Only borrowed tickets can be renewed.");
            }

            var renewalPolicy = _renewalPolicyRepository.GetByOrganization(ticket.OrganizationId);
            if (renewalPolicy == null || !renewalPolicy.AllowRenewals)
            {
                throw new InvalidOperationException("Renewals are not allowed for this organization.");
            }

            if (ticket.RenewCount >= ticket.MaxRenewals)
            {
                throw new InvalidOperationException($"Maximum renewal limit ({ticket.MaxRenewals}) reached.");
            }

            // Check if overdue
            if (ticket.DueAt.HasValue && DateTime.Now > ticket.DueAt.Value)
            {
                throw new InvalidOperationException("Cannot renew overdue tickets.");
            }

            // Extend due date
            ticket.DueAt = ticket.DueAt.Value.AddDays(ticket.BorrowDays);
            ticket.RenewCount++;
            ticket.UpdatedTime = DateTime.Now;
            ticket.UpdatedBy = System.Environment.UserName;

            _ticketRepository.UpdateTicket(ticket);
        }

        public void PayFine(int ticketId)
        {
            var ticket = _ticketRepository.GetById(ticketId);
            if (ticket == null)
            {
                throw new InvalidDataException("Ticket not found.");
            }

            if (ticket.AccruedFine <= 0)
            {
                throw new InvalidOperationException("No fine to pay.");
            }

            ticket.IsFinePaid = true;
            ticket.UpdatedTime = DateTime.Now;
            ticket.UpdatedBy = System.Environment.UserName;

            _ticketRepository.UpdateTicket(ticket);
        }

        // Helper methods
        private List<BorrowTicketViewModel> MapTicketsToViewModels(List<BorrowTicket> tickets)
        {
            return tickets.Select(t => MapTicketToViewModel(t)).ToList();
        }

        private BorrowTicketViewModel MapTicketToViewModel(BorrowTicket ticket)
        {
            var viewModel = _mapper.Map<BorrowTicketViewModel>(ticket);

            // Populate book details
            var book = _booksRepository.GetById(ticket.OrgBookId);
            if (book != null)
            {
                viewModel.BookTitle = book.Title;
                viewModel.BookAuthor = book.Author;
            }

            // Populate user name
            var user = _userRepository.GetUsers().FirstOrDefault(u => u.Id == ticket.UserId);
            if (user != null)
            {
                viewModel.UserName = user.Name;
            }

            // Populate employee names
            if (ticket.ClaimedByEmployeeId.HasValue)
            {
                var claimedBy = _adminRepository.GetAdmins().FirstOrDefault(a => a.Id == ticket.ClaimedByEmployeeId.Value);
                if (claimedBy != null)
                {
                    viewModel.ClaimedByEmployeeName = claimedBy.AdminUsername;
                }
            }

            if (ticket.CheckedOutByEmployeeId.HasValue)
            {
                var checkedOutBy = _adminRepository.GetAdmins().FirstOrDefault(a => a.Id == ticket.CheckedOutByEmployeeId.Value);
                if (checkedOutBy != null)
                {
                    viewModel.CheckedOutByEmployeeName = checkedOutBy.AdminUsername;
                }
            }

            if (ticket.ReturnedToEmployeeId.HasValue)
            {
                var returnedTo = _adminRepository.GetAdmins().FirstOrDefault(a => a.Id == ticket.ReturnedToEmployeeId.Value);
                if (returnedTo != null)
                {
                    viewModel.ReturnedToEmployeeName = returnedTo.AdminUsername;
                }
            }

            // Set status display
            viewModel.StatusDisplay = GetStatusDisplay(ticket.Status);

            return viewModel;
        }

        private string GetStatusDisplay(TicketStatus status)
        {
            return status switch
            {
                TicketStatus.NotYetClaimed => "Not Yet Claimed",
                TicketStatus.Claimed => "Claimed",
                TicketStatus.NotYetReturned => "Not Yet Returned",
                TicketStatus.Returned => "Returned",
                TicketStatus.Cancelled => "Cancelled",
                TicketStatus.Expired => "Expired",
                TicketStatus.Lost => "Lost",
                _ => "Unknown"
            };
        }
    }
}



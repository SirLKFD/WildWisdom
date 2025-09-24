using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository menuRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MenuService> logger;
        private readonly IUnitOfWork uow;

        public MenuService(IMenuRepository repository, IMapper mapper, ILogger<MenuService> logger, IUnitOfWork uow)
        {
            this._mapper = mapper;
            this.menuRepository = repository;
            this.logger = logger;
            this.uow = uow;
        }

        /// <summary>
        /// Retrieves all menu.
        /// </summary>
        /// <returns>Returns IEnumerable menu.</returns>
        public IEnumerable<MenuReturnViewModel> GetAllMenu()
        {
            return _mapper.Map<List<MenuReturnViewModel>>(menuRepository.GetAllMenu());
        }

        /// <summary>
        /// Insertion method for menu.
        /// </summary>
        /// <param name="inputRequest"></param>
        /// <param name="userId"></param>
        /// <returns>Status Code</returns>
        public int AddMenu(MenuRequestViewModel inputRequest, int userId)
        {
            try
            {
                // Check the references first to prevent unnecessary process.
                var checkUnique = menuRepository.CheckUniqueMenu(inputRequest.ID, inputRequest.Name);

                if (CheckMenuRequestInput(inputRequest))
                {
                    //if (checkUnique != ValidationConstants.InputStatus.DuplicateExist)
                    if (checkUnique != -2)
                    {
                        // Get the entity to be added
                        var toAddEntity = _mapper.Map<Menu>(inputRequest);

                        // For new entities, ensure ID is 0 so database generates it
                        if (inputRequest.ID == 0)
                        {
                            toAddEntity.MenuID = 0;
                        }

                        // Save changes to entity & junction tables
                        menuRepository.AddMenu(toAddEntity);

                        // Save changes to logs
                        var logs = CreateLogMenu(userId, toAddEntity, AppConstants.LogTypes.LogAdd, null);
                        menuRepository.AddLogList(logs);
                        return AppConstants.CrudStatusCodes.Success;
                    }
                    else
                    {
                        return AppConstants.CrudStatusCodes.DuplicateExist;
                    }
                }
                else
                {
                    return AppConstants.CrudStatusCodes.DoesNotExist;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw;
            }
            finally
            {
                uow.Dispose();
            }
        }

        public int[] BatchDeleteDocument(IEnumerable<int> inputDocumentIds, int userId)
        {
            try
            {
                // Find the entities to be deleted
                var entities = menuRepository.GetRecordsByIds(inputDocumentIds);

                // Initialize the return values to be at the entities count or least the count of input IDs
                var returnValue = new int[entities.Any() ? entities.Count() : inputDocumentIds.Count()];
                var i = 0;

                // Check the if entity list is not empty to prevent unnecessary process.
                if (entities == null || !entities.Any())
                {
                    Array.Fill(returnValue, AppConstants.CrudStatusCodes.DoesNotExist);
                }
                else
                {
                    // Loop through the entities to be deleted
                    foreach (var entity in entities)
                    {

                        // Update the IsDeleted property
                        entity.IsDeleted = true;

                        // Save changes to batch delete
                        menuRepository.DeleteDocument(entity);

                        // Save changes to logs
                        var logs = CreateLogMenu(userId, entity, AppConstants.LogTypes.LogDelete, null);
                        menuRepository.AddLogList(logs);
                        returnValue[i] = AppConstants.CrudStatusCodes.Success;

                        i++;
                    }
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw;
            }
            finally
            {
                uow.Dispose();
            }
        }

        public bool CheckMenuRequestInput(MenuRequestViewModel inputRequest)
        {
            if (inputRequest == null)
            {
                return false;
            }
            else
            {
                var checkInput = menuRepository.CheckDocumentRequestInput(inputRequest.ID);

                return checkInput;
            }
        }

        public List<LogMenu> CreateLogMenu(int userId, Menu entity, string logType, Dictionary<string, List<string>> unequalProperties)
        {
            // Create log entries for the Document
            var logMenus = new List<LogMenu>();
            var logMenu = new LogMenu()
            {
                LogMenuID = 0,
                Menu = entity,
                UpdatedBy = userId,
                DateCreated = DateTime.Now,
                ActionType = logType,
                MenuID = entity.MenuID,
            };
            if (logType == AppConstants.LogTypes.LogAdd || logType == AppConstants.LogTypes.LogDelete)
            {
                logMenus.Add(logMenu);
            }
            else
            {
                foreach (var unequalProperty in unequalProperties)
                {
                    var changedValue = unequalProperty.Value;
                    var logMenuInLoop = new LogMenu()
                    {
                        LogMenuID = 0,
                        Menu = entity,
                        UpdatedBy = userId,
                        DateCreated = DateTime.Now,
                        ActionType = logType,
                        MenuID = entity.MenuID,
                        FieldChange = unequalProperty.Key,
                        OldValue = changedValue[0],
                        NewValue = changedValue[1],
                    };
                    logMenus.Add(logMenuInLoop);
                }
            }

            return logMenus;
        }
    }
}

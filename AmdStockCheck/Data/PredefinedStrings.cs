using AmdStockCheck.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmdStockCheck.Data
{
    public static class PredefinedStrings
    {
        #region Constant Strings
        public const string cAdd_CannotMessage      = "Cannot send you a private message! ADMIN PLZ FIX ┻━┻ ヘ╰( •̀ε•́ ╰)";
        public const string cAdd_UrlCheckFailed     = "Failed to validate URL! Product Id is probably wrong. 乁| ･ 〰 ･ |ㄏ";
        public const string cAdd_AlreadyRegisterd   = "Product already registered for this User! ། – _ – །";
        public const string cAdd_Ok                 = "Added successfully! ୧༼ ヘ ᗜ ヘ ༽୨";

        public const string cRemove_NotRegistered   = "You are not registered for this product! ୧( ಠ Д ಠ )୨";
        public const string cRemove_Ok              = "Removed you from mention list! ᕕ( ཀ ʖ̯ ཀ)ᕗ";

        public const string cGeneral_InternalError  = "Product already registered for this User! ། – _ – །";

        public const string cService_Available      = "Available: {0}\n{1}";
        public const string cService_QueueStarted   = "Queue started: {0}\n{1}";
        public const string cService_RequestError   = "Something's not quite right...\n{0}";
        #endregion

        #region Getter
        public static string GetString(AmdStockCheckService.RegisterReturnState state)
        {
            return (state switch
            {
                AmdStockCheckService.RegisterReturnState.CannotMessage => cAdd_CannotMessage,
                AmdStockCheckService.RegisterReturnState.UrlCheckFailed => cAdd_UrlCheckFailed,
                AmdStockCheckService.RegisterReturnState.AlreadyRegistered => cAdd_AlreadyRegisterd,
                AmdStockCheckService.RegisterReturnState.Ok => cAdd_Ok,
                _ => cGeneral_InternalError
            });
        }
        public static string GetString(AmdStockCheckService.UnregisterReturnState state)
        {
            return (state switch
            {
                AmdStockCheckService.UnregisterReturnState.NotRegistered => cRemove_NotRegistered,
                AmdStockCheckService.UnregisterReturnState.Ok => cRemove_Ok,
                _ => cGeneral_InternalError
            });
        }
        #endregion
    }
}

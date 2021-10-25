using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmdStockCheck.Data
{
    public static class PredefinedStrings
    {
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
    }
}

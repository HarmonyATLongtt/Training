using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace FirstCommand.Support.TransactionHandle
{
    public static class HandleForTransaction
    {
        /// <summary>
        /// Hàm để runtransaction
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="transactionName"></param>
        /// <param name="action"></param>
        public static void RunTransaction(Document doc, string transactionName, Action<Transaction> action)
        {
            using (Transaction trans = new Transaction(doc, transactionName))
            {
                trans.Start();

                FailureHandlingOptions options = trans.GetFailureHandlingOptions();
                options.SetFailuresPreprocessor(new WarningSuppressor());
                trans.SetFailureHandlingOptions(options);

                action(trans); // Thực thi hành động trong Transaction

                trans.Commit();
            }
        }
    }
}
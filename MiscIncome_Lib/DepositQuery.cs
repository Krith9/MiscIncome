using QBFC16Lib;

namespace MiscIncome_Lib
{
    public class DepositQuery
    {
        public List<DepositRecord> GetDeposits()
        {
            List<DepositRecord> deposits = new List<DepositRecord>();
            bool sessionBegun = false;
            bool connectionOpen = false;
            QBSessionManager sessionManager = null;

            try
            {
                sessionManager = new QBSessionManager();
                IMsgSetRequest requestMsgSet = sessionManager.CreateMsgSetRequest("US", 16, 0);
                requestMsgSet.Attributes.OnError = ENRqOnError.roeContinue;

                IDepositQuery depositQuery = requestMsgSet.AppendDepositQueryRq();
                depositQuery.IncludeLineItems.SetValue(true);

                sessionManager.OpenConnection("", "Deposit Query");
                connectionOpen = true;
                sessionManager.BeginSession("", ENOpenMode.omDontCare);
                sessionBegun = true;

                IMsgSetResponse responseMsgSet = sessionManager.DoRequests(requestMsgSet);
                sessionManager.EndSession();
                sessionBegun = false;
                sessionManager.CloseConnection();
                connectionOpen = false;

                IResponseList responseList = responseMsgSet.ResponseList;
                if (responseList != null)
                {
                    for (int i = 0; i < responseList.Count; i++)
                    {
                        IResponse response = responseList.GetAt(i);
                        if (response.StatusCode == 0 && response.Detail is IDepositRetList depositRetList)
                        {
                            for (int j = 0; j < depositRetList.Count; j++) // FIX: Use index-based access
                            {
                                IDepositRet depositRet = depositRetList.GetAt(j);
                                deposits.Add(new DepositRecord
                                {
                                    TxnID = depositRet.TxnID.GetValue(),
                                    Memo = depositRet.Memo?.GetValue(),
                                    DepositTotal = depositRet.DepositTotal?.GetValue() ?? 0.0
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
            finally
            {
                if (sessionBegun) sessionManager?.EndSession();
                if (connectionOpen) sessionManager?.CloseConnection();
            }

            return deposits;
        }
    }

    public class DepositRecord
    {
        public string TxnID { get; set; }
        public string Memo { get; set; }
        public double DepositTotal { get; set; }
    }
}

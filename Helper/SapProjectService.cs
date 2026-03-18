using ProjectWBSAPI.Model;
using SAP.Middleware.Connector;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProjectWBSAPI.Helper
{
    public class SapProjectService
    {
        private readonly SapConnectionService _sapConnection;

        public SapProjectService(SapConnectionService sapConnection)
        {
            _sapConnection = sapConnection;
        }

        public List<ProjectDto> GetProjects() 
        {
            var destination = _sapConnection.GetDestination();

            var repository = destination.Repository;

            IRfcFunction function = repository.CreateFunction("Z_PS_FM_TIMETOOL");

            DateTime startDate = Convert.ToDateTime("01-JAN-2020");// DateTime.Now.AddDays(-1);
            function.SetValue("FROM_DATE", startDate.ToString("yyyyMMdd"));
            DateTime endDate = Convert.ToDateTime("25-FEB-2020"); //DateTime.Now;
            function.SetValue("TO_DATE", endDate.ToString("yyyyMMdd"));

            function.Invoke(destination);

            IRfcTable tblProj = function.GetTable("LT_PROJ");
            //IRfcTable tblwbs = function.GetTable("LT_WBS");
            //return "Ok";
            var orders = new List<ProjectDto>();

            foreach (IRfcStructure row in tblProj)
            {
                orders.Add(new ProjectDto
                {
                    ProjectCode = row.GetString("PSPID"),
                    ProjectDescription = row.GetString("POST1"),
                    BU = row.GetString("ZDIV")
                });
            }

            return orders;
        }

        public List<WBSDto> GetWBS() 
        {
            var destination = _sapConnection.GetDestination();

            var repository = destination.Repository;

            IRfcFunction function = repository.CreateFunction("Z_PS_FM_TIMETOOL");

            DateTime startDate = Convert.ToDateTime("01-JAN-2020");// DateTime.Now.AddDays(-1);
            function.SetValue("FROM_DATE", startDate.ToString("yyyyMMdd"));
            DateTime endDate = Convert.ToDateTime("25-FEB-2020"); //DateTime.Now;
            function.SetValue("TO_DATE", endDate.ToString("yyyyMMdd"));

            function.Invoke(destination);

            //IRfcTable tblProj = function.GetTable("LT_PROJ");
            IRfcTable tblwbs = function.GetTable("LT_WBS");
            //return "Ok";
            var wbs = new List<WBSDto>();

            foreach (IRfcStructure row in tblwbs) 
            {
                wbs.Add(new WBSDto
                {
                    WBSCode = row.GetString("POSID"),
                    WBSName = row.GetString("POST1"),
                    ProjectName = row.GetString("PSPHI"),
                    Superior = row.GetString("UP")
                });
            }

            return wbs;
        }
    }
}

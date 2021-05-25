using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furnas.GestaoSPE.Unificacao.Base.Infrastruture
{
    public static class LogCustom
    {
        public static void WriteLog(string source, TraceSeverity traceSeverity, EventSeverity eventSeverity, string logMessage)
        {
            try
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(source, traceSeverity, eventSeverity), traceSeverity, logMessage, null);
            }
            catch (Exception)
            {
                // maybe write to Event Log3?
            }
        }
    }
}

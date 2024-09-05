using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using ServiceNowWeb.Models;
using System.Text;
using System.Drawing;
using RestSharp;
using System.Text.Json.Nodes;
using Newtonsoft.Json;


namespace ServiceNowWeb.Controllers
{
    public class ChamadosServiceNowController : Controller
    {
        private readonly ILogger<ChamadosServiceNowController> _logger;

        //public static List<Tuple<string, List<KeyValuePair<Tuple<string, string>, int>>>> dicChamadosValidos = new List<Tuple<string, List<KeyValuePair<Tuple<string, string>, int>>>>();
        public static List<Tuple<Tuple<string, string>, List<KeyValuePair<Tuple<string, string>, int>>>> dicChamadosValidos = new List<Tuple<Tuple<string, string>, List<KeyValuePair<Tuple<string, string>, int>>>>();

        public ChamadosServiceNowController(ILogger<ChamadosServiceNowController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public string ProcessarPlanilhaChamados(string pathPlanilhaAlarmes, int intervalo, int quantidade)
        {
            StringBuilder sb = new StringBuilder();
            List<Chamado> lstChamados = new List<Chamado>();

            Dictionary<Tuple<string, string>, Dictionary<Tuple<string, string>, int>> dicChamados = new Dictionary<Tuple<string, string>, Dictionary<Tuple<string, string>, int>>();
            Dictionary<Tuple<string, string>, int> dicChamadosCorrelatos = new Dictionary<Tuple<string, string>, int>();

            FileInfo planilhaAlarmes = new FileInfo(pathPlanilhaAlarmes);

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage(planilhaAlarmes))
            {
                var wsAlarmes = package.Workbook.Worksheets["Alarmes"];

                for (int i = 2; i <= wsAlarmes.Dimension.End.Row; i++)
                //for (int i = 2; i <= 1000; i++)
                {
                    var dataCriacao = Convert.ToDateTime(wsAlarmes.Cells[i, 2].Text);
                    var host = wsAlarmes.Cells[i, 5].Text;
                    var descricaoResumida = wsAlarmes.Cells[i, 6].Text;

                    lstChamados.Add(new Chamado() { DataCriacao = dataCriacao, Host = host, DescricaoResumida = descricaoResumida });
                }
            }

            lstChamados = lstChamados.OrderBy(x => x.DataCriacao).ToList();

            foreach (var chamado in lstChamados)
            {
                var host = chamado.Host!;

                List<Chamado> lstChamadosCorrelatos = new List<Chamado>();

                lstChamadosCorrelatos = lstChamados.Where(x => x.DataCriacao > chamado.DataCriacao && x.DataCriacao <= chamado.DataCriacao.AddSeconds(intervalo)).ToList();

                Tuple<string, string> compositeKeyPai = Tuple.Create(host, chamado.DescricaoResumida);

                if (dicChamados.ContainsKey(compositeKeyPai))
                    dicChamadosCorrelatos = dicChamados[compositeKeyPai];
                else
                {
                    dicChamados.Add(compositeKeyPai, new Dictionary<Tuple<string, string>, int>());
                    dicChamadosCorrelatos = dicChamados[compositeKeyPai];
                }

                foreach (var chamadoCorrelato in lstChamadosCorrelatos)
                {
                    string hostCorrelato = chamadoCorrelato.Host!;
                    string descricaoResumida = chamadoCorrelato.DescricaoResumida;

                    Tuple<string, string> compositeKey = Tuple.Create(hostCorrelato, descricaoResumida);

                    if (dicChamadosCorrelatos.ContainsKey(compositeKey))
                    {
                        dicChamadosCorrelatos[compositeKey]++;
                    }
                    else
                        dicChamadosCorrelatos.Add(compositeKey, 1);
                }
            }

            foreach (var dic in dicChamados)
            {
                var lstCorrelatosValidos = dic.Value.Where(x => x.Value > quantidade).ToList();

                if (lstCorrelatosValidos == null || !lstCorrelatosValidos.Any())
                    continue;

                dicChamadosValidos.Add(Tuple.Create(dic.Key, lstCorrelatosValidos));

                sb.Append("<ul>");
                sb.Append("<li>");
                sb.Append($"<b>{dic.Key.Item1}</b> => {dic.Key.Item2} - <b class='text-primary'>{lstCorrelatosValidos.Sum(x => x.Value)}</b>");
                sb.Append("<ul>");
                foreach (var dic2 in lstCorrelatosValidos.OrderByDescending(x => x.Value))
                {
                    sb.Append($"<li><b><span class='text-danger'>{dic2.Value.ToString().PadLeft(2, '0')}</span></b> - <b>{dic2.Key.Item1}</b> => {dic2.Key.Item2}</li>");
                }
                sb.Append("</ul>");
                sb.Append("</li>");
                sb.Append("</ul>");
            }

            return sb.ToString();
        }

        //[HttpGet]
        //public string ProcessarPlanilhaChamados(string pathPlanilhaAlarmes, int intervalo, int quantidade)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    List<Chamado> lstChamados = new List<Chamado>();
        //    Dictionary<string, Dictionary<Tuple<string, string>, int>> dicChamados = new Dictionary<string, Dictionary<Tuple<string, string>, int>>();
        //    Dictionary<Tuple<string, string>, int> dicChamadosCorrelatos = new Dictionary<Tuple<string, string>, int>();

        //    FileInfo planilhaAlarmes = new FileInfo(pathPlanilhaAlarmes);

        //    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        //    using (ExcelPackage package = new ExcelPackage(planilhaAlarmes))
        //    {
        //        var wsAlarmes = package.Workbook.Worksheets["Alarmes"];

        //        for (int i = 2; i <= wsAlarmes.Dimension.End.Row; i++)
        //        //for (int i = 2; i <= 1000; i++)
        //        {
        //            var dataCriacao = Convert.ToDateTime(wsAlarmes.Cells[i, 2].Text);
        //            var host = wsAlarmes.Cells[i, 5].Text;
        //            var descricaoResumida = wsAlarmes.Cells[i, 6].Text;

        //            lstChamados.Add(new Chamado() { DataCriacao = dataCriacao, Host = host, DescricaoResumida = descricaoResumida });
        //        }
        //    }

        //    lstChamados = lstChamados.OrderBy(x => x.DataCriacao).ToList();

        //    foreach (var chamado in lstChamados)
        //    {
        //        var host = chamado.Host!;

        //        List<Chamado> lstChamadosCorrelatos = new List<Chamado>();

        //        lstChamadosCorrelatos = lstChamados.Where(x => x.DataCriacao > chamado.DataCriacao && x.DataCriacao <= chamado.DataCriacao.AddSeconds(intervalo)).ToList();

        //        if (dicChamados.ContainsKey(host))
        //            dicChamadosCorrelatos = dicChamados[host];
        //        else
        //        {
        //            dicChamados.Add(host, new Dictionary<Tuple<string, string>, int>());
        //            dicChamadosCorrelatos = dicChamados[host];
        //        }

        //        foreach (var chamadoCorrelato in lstChamadosCorrelatos)
        //        {
        //            string hostCorrelato = chamadoCorrelato.Host!;
        //            string descricaoResumida = chamadoCorrelato.DescricaoResumida;

        //            Tuple<string, string> compositeKey = Tuple.Create(hostCorrelato, descricaoResumida);

        //            if (dicChamadosCorrelatos.ContainsKey(compositeKey))
        //            {
        //                dicChamadosCorrelatos[compositeKey]++;
        //            }
        //            else
        //                dicChamadosCorrelatos.Add(compositeKey, 1);
        //        }
        //    }

        //    foreach (var dic in dicChamados)
        //    {
        //        var lstCorrelatosValidos = dic.Value.Where(x => x.Value > quantidade).ToList();

        //        if (lstCorrelatosValidos == null || !lstCorrelatosValidos.Any())
        //            continue;

        //        dicChamadosValidos.Add(Tuple.Create(dic.Key, lstCorrelatosValidos));

        //        sb.Append("<ul>");
        //        sb.Append("<li>");
        //        sb.Append($"{dic.Key} - <b class='text-primary'>{lstCorrelatosValidos.Sum(x => x.Value)}</b>");
        //        sb.Append("<ul>");
        //        foreach (var dic2 in lstCorrelatosValidos.OrderByDescending(x => x.Value))
        //        {
        //            sb.Append($"<li><b><span class='text-danger'>{dic2.Value.ToString().PadLeft(2, '0')}</span></b> - <b>{dic2.Key.Item1}</b> => {dic2.Key.Item2}</li>");
        //        }
        //        sb.Append("</ul>");
        //        sb.Append("</li>");
        //        sb.Append("</ul>");
        //    }

        //    return sb.ToString();
        //}

        [HttpGet]
        public string ProcessarChamadosApi()
        {
            List<AlertResult> lstAlertResult = new List<AlertResult>();

            RestClient client = new RestClient("https://petrobrasdevtic.service-now.com/api/now/table/em_alert");

            RestRequest request = new RestRequest();

            request.Method = Method.Get;
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Apikey", "9c55e2c813814b28a328e4d9800ad021");
            request.AddHeader("Authorization", "Basic U0EwMUNPUlM6QGNvMDFSUyE=");

            request.AddParameter("sysparm_limit", "100");
            request.AddParameter("source", "Zabbix");
            request.AddParameter("sysparm_fields", "number,sys_created_on,node,short_description,cmdb_ci,metric_name");
            request.AddParameter("sysparm_offset", "100000");

            var response = client.Execute(request);

            if (!response.IsSuccessStatusCode)
                return string.Empty;

            var lstAlert = JsonConvert.DeserializeObject<AlertServiceNow>(response.Content!);

            request = new RestRequest();

            request.Method = Method.Get;
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Apikey", "9c55e2c813814b28a328e4d9800ad021");
            request.AddHeader("Authorization", "Basic U0EwMUNPUlM6QGNvMDFSUyE=");

            foreach (var alert in lstAlert!.result)
            {
                if (alert == null || alert.cmdb_ci == null)
                    continue;

                client = new RestClient(alert.cmdb_ci!.link!);

                response = client.Execute(request);

                if (!response.IsSuccessStatusCode)
                    continue;

                var cmdb_ci = JsonConvert.DeserializeObject<Cmdb_ci_ServiceNow>(response.Content!);

                if (cmdb_ci == null || cmdb_ci.result.location == null)
                    continue;                

                client = new RestClient(cmdb_ci!.result.location.link!);

                response = client.Execute(request);

                if (!response.IsSuccessStatusCode)
                    continue;

                var location = JsonConvert.DeserializeObject<LocationServiceNow>(response.Content!);

                alert.ip_address = cmdb_ci != null ? cmdb_ci.result.ip_address : string.Empty;
                alert.name = location != null ? location.result.name : string.Empty;

                lstAlertResult.Add(new AlertResult() 
                { 
                    number = alert.number,
                    node = alert.node,
                    short_description = alert.short_description,
                    sys_created_on = alert.sys_created_on,
                    ip_address = alert.ip_address,
                    name = alert.name
                });
            }

            var retorno = JsonConvert.SerializeObject(lstAlertResult, Formatting.Indented);

            return string.Empty;
        }

        public void ExportarChamadosCorrelatos()
        {
            int linha = 2;

            FileInfo planilha = new FileInfo("C:\\Francisco\\Projetos\\ServiceNowWeb\\Correlação_Chamados.xlsx");

            if (planilha.Exists)
                planilha.Delete();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (ExcelPackage excelPackage = new ExcelPackage(planilha))
            {
                var ws = excelPackage.Workbook.Worksheets.Add("CHAMADOS_CORRELATOS");

                ws.Cells[1, 1].Value = "Host";
                ws.Cells[1, 2].Value = "Host Correlato";
                ws.Cells[1, 3].Value = "Descrição";
                ws.Cells[1, 4].Value = "Total";

                ws.Cells[1, 1, 1, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[1, 1, 1, 4].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                ws.Cells[1, 1, 1, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[1, 1, 1, 4].Style.Font.Bold = true;
                ws.Cells.AutoFitColumns();

                foreach (var chamadoValido in dicChamadosValidos)
                {
                    foreach (var item in chamadoValido.Item2)
                    {
                        ws.Cells[linha, 1].Value = chamadoValido.Item1;

                        ws.Cells[linha, 2].Value = item.Key.Item1;
                        ws.Cells[linha, 3].Value = item.Key.Item2;
                        ws.Cells[linha, 4].Value = item.Value;

                        linha++;
                    }
                }

                excelPackage.SaveAsAsync(new FileInfo(planilha.FullName));
            }
        }
    }
}
using Furnas.GestaoSPE.Unificacao.Base.Resources;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furnas.GestaoSPE.Unificacao.Base.Service
{
    public static class ServiceMoveDocs
    {
        public static void MoveDocsSPE(SPWeb web)
        {
            string libDestiny = web.Site.WebApplication.GetResponseUri(SPUrlZone.Default).AbsoluteUri.ToString() + Constants.NameSiteCollectionDocsDest.SiteCollectionDest;

            using (SPSite destSite = new SPSite(libDestiny))
            {
                using (SPWeb destWeb = destSite.OpenWeb())
                {
                    List<string> libs = libraries();
                    foreach (string lib in libs)
                    {
                        SPFolder docOrigem = null;
                        SPFolder dest = null;

                        string lib_url = string.Empty;

                        if (lib == "Documentos Plano de Negócio")
                        {
                            docOrigem = web.GetFolder("Documentos PN");
                            dest = destWeb.GetFolder("Documentos Plano de Negcio");

                            lib_url = "Documentos Plano de Negcio";
                        }
                        else if (lib == "Documentos Balanço Patrimonial")
                        {
                            docOrigem = web.GetFolder("Documentos Documento Balano Patrimonial");
                            dest = destWeb.GetFolder("Documentos Balano Patrimonial");

                            lib_url = "Documentos Balano Patrimonial";
                        }
                        else if (lib == "Documento Dividendos SPE")
                        {
                            docOrigem = web.GetFolder("Documento Dividendos SPE");
                            dest = destWeb.GetFolder("Documentos Dividendos SPE");

                            lib_url = "Documentos Dividendos SPE";
                        }
                        else if (lib == "Documento Aporte SPE")
                        {
                            docOrigem = web.GetFolder("Documento Aporte SPE");
                            dest = destWeb.GetFolder("Documentos Aporte SPE");

                            lib_url = "Documentos Aporte SPE";
                        }
                        else if (lib == "Documento Financiamento SPE")
                        {
                            docOrigem = web.GetFolder("Documento Financiamento SPE");
                            dest = destWeb.GetFolder("Documentos Financiamento SPE");

                            lib_url = "Documentos Financiamento SPE";
                        }
                        else if (lib == "Documento Pessoa")
                        {
                            docOrigem = web.GetFolder("Documento Pessoa");
                            dest = destWeb.GetFolder("Documentos Pessoa");

                            lib_url = "Documentos Pessoa";
                        }
                        else if (lib == "Documentos Licença")
                        {
                            docOrigem = web.GetFolder("Documentos Licena");
                            dest = destWeb.GetFolder("Documentos Licena");

                            lib_url = "Documentos Licena";
                        }
                        else if (lib == "Documentos Remuneração Global")
                        {
                            docOrigem = web.GetFolder("Documentos Remunerao Global");
                            dest = destWeb.GetFolder("Documentos Remunerao Global");

                            lib_url = "Documentos Remunerao Global";
                        }
                        else
                        {
                            docOrigem = web.GetFolder(lib);
                            dest = destWeb.GetFolder(lib);
                        }
                        
                        //SPFolder dest = destWeb.GetFolder(lib);
                        SPFile file = null;

                        int file_count = docOrigem.Files.Count - 1;
                        for (int i = file_count; i >= 0; i--)
                        {
                            try
                            {
                                file = docOrigem.Files[i];

                                if (file.CheckOutStatus != SPFile.SPCheckOutStatus.None)
                                {
                                    file.CheckIn(string.Empty);
                                }

                                try
                                {
                                    if (dest.Files[file.Name].Exists)
                                    {
                                        if (dest.Files[file.Name].CheckOutStatus == SPFile.SPCheckOutStatus.None)
                                        {
                                            dest.Files[file.Name].CheckOut();
                                        }
                                    }
                                }
                                catch { }

                                SPFile f = dest.Files.Add(file.Name.Replace(",", ""), file.OpenBinary(), true);

                                SPListItem item = f.Item;
                                UpdateProprieties(item, file, destWeb, lib);
                                
                                if (f.CheckOutStatus != SPFile.SPCheckOutStatus.None)
                                {
                                    f.CheckIn(string.Empty);
                                }

                                //ChangeName(item, file);

                                //ListSPE(web, file, destWeb.Url + "/" + f.Url, lib, item.ID);
                                ListSPE(web, file, destWeb.Url + "/", lib, item, lib_url);

                                DeleteFile(file);
                            }
                            catch (Exception err)
                            {
                                ServicePermissaoEsclusivaMassa.WriteLog(web, "Mover Item - Lista: " + dest.DocumentLibrary.Title + " ID do Item: " + file.Item.ID, err.Message);
                            }
                        }
                    }
                }
            }
        }

        public static void ChangeName(SPListItem item, string nome)
        {
            item["Nome"] = nome;
            item.Update();
        }

        public static void DeleteFile(SPFile file)
        {
            file.Delete();
        }

        public static void ListSPE(SPWeb web, SPFile file, string url, string lib, SPListItem doc_item, string url_singular)
        {
            SPListItem item = web.Lists[Constants.NameLibraryLegado.ListSPE].Items.Add();

            switch (lib)
            {
                case "Documentos Contrato":
                    item["Complexo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Complexo"]));
                    item["SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["SPE"]));
                    item["Contrato SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Contrato SPE"]));
                    item["Tipo Documento Contrato"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Tipo Documento Contrato"]));
                    item["Descrição"] = Util.ValidaTextField(file.Item["Descrição"]);
                    item[SPBuiltInFieldId.ContentTypeId] = web.Lists[Constants.NameLibraryLegado.ListSPE].ContentTypes["Documentos Contrato"].Id;
                    break;
                case "Documento Aporte SPE":
                    item["Complexo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Complexo"]));
                    item["SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["SPE"]));
                    item["Aporte SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Aporte SPE"]));
                    item["Tipo Documento Aporte SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Tipo Documento Aporte SPE"]));
                    item["Descrição"] = Util.ValidaTextField(file.Item["Descrição"]);
                    item[SPBuiltInFieldId.ContentTypeId] = web.Lists[Constants.NameLibraryLegado.ListSPE].ContentTypes["Documentos Aporte SPE"].Id;
                    break;
                case "Anexo":
                    item["Processo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Processo"]));
                    item["Fase"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Fase"]));
                    item["Modelo Anexo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Modelo Anexo"]));
                    item[SPBuiltInFieldId.ContentTypeId] = web.Lists[Constants.NameLibraryLegado.ListSPE].ContentTypes["Anexo"].Id;
                    break;
                case "Documentos Acompanhamento Empreendimento":
                    item["Complexo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Complexo"]));
                    item["SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["SPE"]));
                    item["Empreendimento"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Empreendimento"]));
                    item["Acompanhamento Empreendimento"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Acompanhamento Empreendimento"]));
                    item["Tipo Documento Acompanhamento Empreendimento"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Tipo Documento Acompanhamento Empreendimento"]));
                    item["Descrição"] = Util.ValidaTextField(file.Item["Descrição"]);
                    item[SPBuiltInFieldId.ContentTypeId] = web.Lists[Constants.NameLibraryLegado.ListSPE].ContentTypes["Documentos Acompanhamento Empreendimento"].Id;
                    break;
                case "Documentos Acompanhamento Obra":
                    item["Complexo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Complexo"]));
                    item["SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["SPE"]));
                    item["Empreendimento"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Empreendimento"]));
                    item["Obra"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Obra"]));
                    item["Acompanhamento Obra"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Acompanhamento Obra"]));
                    item["Tipo Documento Acompanhamento Obra"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Tipo Documento Acompanhamento Obra"]));
                    item["Descrição"] = Util.ValidaTextField(file.Item["Descrição"]);
                    item[SPBuiltInFieldId.ContentTypeId] = web.Lists[Constants.NameLibraryLegado.ListSPE].ContentTypes["Documentos Acompanhamento Obra"].Id;
                    break;
                case "Documentos Balanço Patrimonial":
                    item["Complexo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Complexo"]));
                    item["SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["SPE"]));
                    item["Balanço Patrimonial"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Balanço Patrimonial"]));
                    item["Tipo Documento Balanço Patrimonial"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Tipo Documento Balanço Patrimonial"]));
                    item["Descrição"] = Util.ValidaTextField(file.Item["Descrição"]);
                    item[SPBuiltInFieldId.ContentTypeId] = web.Lists[Constants.NameLibraryLegado.ListSPE].ContentTypes["Documentos Balanço Patrimonial"].Id;
                    break;
                case "Documento Dividendos SPE":
                    item["Complexo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Complexo"]));
                    item["SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["SPE"]));
                    if(Util.ValidaTextField(file.Item["Dividendos SPE"]) != string.Empty)
                        item["Dividendos SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Dividendos SPE"]));
                    item["fld_tipoDividendosSPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Tipo Documento Dividendos SPE"]));
                    item["Descrição"] = Util.ValidaTextField(file.Item["Descrição"]);
                    item[SPBuiltInFieldId.ContentTypeId] = web.Lists[Constants.NameLibraryLegado.ListSPE].ContentTypes["Documentos Dividendos SPE"].Id;
                    break;
                case "Documentos Empreendimento":
                    item["Complexo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Complexo"]));
                    item["SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["SPE"]));
                    item["Empreendimento"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Empreendimento"]));
                    item["Tipo Documento Empreendimento"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Tipo Documento Empreendimento"]));
                    item["Descrição"] = Util.ValidaTextField(file.Item["Descrição"]);
                    item[SPBuiltInFieldId.ContentTypeId] = web.Lists[Constants.NameLibraryLegado.ListSPE].ContentTypes["Documentos Empreendimento"].Id;
                    break;
                case "Documento Financiamento SPE":
                    item["Complexo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Complexo"]));
                    item["SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["SPE"]));
                    item["Financiamento SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Financiamento SPE"]));
                    item["Tipo Documento Financiamento SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Tipo Documento Financiamento SPE"]));
                    item["Descrição"] = Util.ValidaTextField(file.Item["Descrição"]);
                    item[SPBuiltInFieldId.ContentTypeId] = web.Lists[Constants.NameLibraryLegado.ListSPE].ContentTypes["Documentos Financiamento SPE"].Id;
                    break;
                case "Documentos Licença":
                    item["Complexo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Complexo"]));
                    item["SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["SPE"]));
                    item["Empreendimento"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Empreendimento"]));
                    item["Licença Empreendimento"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Licença Empreendimento"]));
                    item["Tipo Documento Licença"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Tipo Documento Licença"]));
                    item["Descrição"] = Util.ValidaTextField(file.Item["Descrição"]);
                    item[SPBuiltInFieldId.ContentTypeId] = web.Lists[Constants.NameLibraryLegado.ListSPE].ContentTypes["Documentos Licença"].Id;
                    break;
                case "Documentos Obra":
                    item["Complexo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Complexo"]));
                    item["SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["SPE"]));
                    item["Empreendimento"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Empreendimento"]));
                    item["Obra"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Obra"]));
                    item["Tipo Documento Obra"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Tipo Documento Obra"]));
                    item["Descrição"] = Util.ValidaTextField(file.Item["Descrição"]);
                    item[SPBuiltInFieldId.ContentTypeId] = web.Lists[Constants.NameLibraryLegado.ListSPE].ContentTypes["Documentos Obra"].Id;
                    break;
                case "Documento Pessoa":
                    item["Pessoa"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Pessoa"]));
                    item["Tipo Documento Pessoa"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Tipo Documento Pessoa"]));
                    item["Descrição"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Descrição"]));
                    item[SPBuiltInFieldId.ContentTypeId] = web.Lists[Constants.NameLibraryLegado.ListSPE].ContentTypes["Documentos Pessoa"].Id;
                    break;
                case "Documentos Plano de Negócio":
                    item["Complexo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Complexo"]));
                    item["SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["SPE"]));
                    item["Empreendimento"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Empreendimento"]));
                    item["PN"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["PN"]));
                    item["Tipo Documento Plano de Negócio"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Tipo Documento Plano de Negócio"]));
                    item["Descrição"] = Util.ValidaTextField(file.Item["Descrição"]);
                    item[SPBuiltInFieldId.ContentTypeId] = web.Lists[Constants.NameLibraryLegado.ListSPE].ContentTypes["Documentos Plano de Negócio"].Id;
                    break;
                case "Documentos Remuneração Global":
                    item["SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["SPE"]));
                    item["Grupo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Grupo"]));
                    if (Util.ValidaTextField(file.Item["Tipo_x0020_de_x0020_Remunera_x00e7__x00e3_o_x0020_Global"]) != "")
                    {
                        item["fld_tipoRemuneracaoGlobal"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Tipo_x0020_de_x0020_Remunera_x00e7__x00e3_o_x0020_Global"]));
                    }
                    item[SPBuiltInFieldId.ContentTypeId] = web.Lists[Constants.NameLibraryLegado.ListSPE].ContentTypes["Documentos Remuneração Global"].Id;
                    break;
                case "Documentos SPE":
                    item["Complexo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Complexo"]));
                    item["SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["SPE"]));
                    item["Tipo Documento SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Tipo Documento SPE"]));
                    item["Descrição"] = Util.ValidaTextField(file.Item["Descrição"]);
                    item[SPBuiltInFieldId.ContentTypeId] = web.Lists[Constants.NameLibraryLegado.ListSPE].ContentTypes["Documentos SPE"].Id;
                    break;
                default:
                    break;
            }

            //item[SPBuiltInFieldId.Title] = Util.ValidaTextField(file.Item["Nome"]);
            string nome = Path.GetFileNameWithoutExtension(file.Name.Replace(",", "")) + "_" + Zero(item) + Path.GetExtension(file.Name.Replace(",", ""));
            item[SPBuiltInFieldId.Title] = nome;
            //item["Complexo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Complexo"]));
            //item["Descrição"] = Util.ValidaTextField(file.Item["Descrição"]);
            //item["SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["SPE"]));
            //item["Tipo_x0020_Documento_x0020_SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Tipo_x0020_Documento_x0020_SPE"]));
            //item["Descrição"] = Util.ValidaTextField(file.Item["Descrição"]);
            item["Created"] = Convert.ToDateTime(file.Item[SPBuiltInFieldId.Created_x0020_Date]);
            item["Modified"] = Convert.ToDateTime(file.Item[SPBuiltInFieldId.Modified]);
            if (lib != "Anexo")
            {
                if (Util.ValidaTextField(file.Item["Publicar"]) == "False")
                { item["Publicar"] = false; }
                else
                { item["Publicar"] = true; }

                if (Util.ValidaTextField(file.Item["Ativo"]) == "False")
                { item["Ativo"] = false; }
                else
                { item["Ativo"] = true; }
            }
            SPFieldUserValue user = new SPFieldUserValue(web, file.Item[SPBuiltInFieldId.Author].ToString());
            item[SPBuiltInFieldId.Author] = user.User;

            SPFieldUrlValue link = null;
            if (url_singular == string.Empty)
            {
                 link = new SPFieldUrlValue(url + lib + "/" + nome);
            }
            else
            {
                link = new SPFieldUrlValue(url + url_singular + "/" + nome);
            }
            item["Documento"] = link.Url;

            item.Update();

            ChangeName(doc_item,nome);
        }

        public static string Zero(SPListItem item)
        {
            int id = item.ListItems.Count;

            if (id < 10)
            {
                return "0" + id.ToString();
            }
            else
            {
                return id.ToString();
            }
        }

        public static void UpdateProprieties(SPListItem item, SPFile file, SPWeb web, string contentType)
        {
            //item["Complexo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Complexo"])).LookupValue;//file.Item[""];
            //item["Descrição"] = Util.ValidaTextField(file.Item["Descrição"]);
            //item["SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["SPE"])).LookupValue;
            //item["Sigla Complexo"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Complexo:Sigla"])).LookupValue;
            //item["Apelido SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["SPE:Apelido"])).LookupValue;
            //item["Tipo_x0020_Documento_x0020_SPE"] = new SPFieldLookupValue(Util.ValidaTextField(file.Item["Tipo_x0020_Documento_x0020_SPE"])).LookupValue;
            //item["ID Tipo Documento SPE"] = new SPFieldLookupValue(Convert.ToString(file.Item["Tipo Documento SPE:ID"])).LookupId;
            item["Created"] = Convert.ToDateTime(file.Item[SPBuiltInFieldId.Created_x0020_Date]);
            item["Modified"] = Convert.ToDateTime(file.Item[SPBuiltInFieldId.Modified]);
            //if (Util.ValidaTextField(file.Item["Publicar"]) == "Sim")
            //    item["Publicar"] = true;
            //else
            //    item["Publicar"] = false;
            item["Tipo de Documento"] = contentType;
            SPFieldUserValue user = new SPFieldUserValue(web, file.Item[SPBuiltInFieldId.Author].ToString());
            item[SPBuiltInFieldId.Author] = user.User;

            //item["Nome"] = Path.GetFileNameWithoutExtension(file.Name) + "_" + Zero(item.ID) + Path.GetExtension(file.Name);
            item.Update();
        }

        public static List<string> libraries()
        {
            List<string> libraries = new List<string>();

            libraries.Add("Documentos Contrato");
            libraries.Add("Documento Aporte SPE");
            libraries.Add("Anexo");
            libraries.Add("Documentos Acompanhamento Empreendimento");
            libraries.Add("Documentos Acompanhamento Obra");
            libraries.Add("Documentos Balanço Patrimonial");
            libraries.Add("Documento Dividendos SPE");
            libraries.Add("Documentos Empreendimento");
            libraries.Add("Documento Financiamento SPE");
            libraries.Add("Documentos Licença");
            libraries.Add("Documentos Obra");
            libraries.Add("Documento Pessoa");
            libraries.Add("Documentos Plano de Negócio");
            libraries.Add("Documentos Remuneração Global");
            libraries.Add("Documentos SPE");

            return libraries;
        }
    }
}

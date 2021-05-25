using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Furnas.GestaoSPE.Unificacao.Base.Resources;
using Microsoft.SharePoint.Administration;
using System.IO;

namespace Furnas.GestaoSPE.Unificacao.Base.Service
{
    public class ServiceMoveDocsCurrent : ServicePermissaoExclusiva
    {
        public SPFile _file { get; set; }
        public string _tipoConteudo { get; set; }
        public SPFile _attachments { get; set; }
        public SPItemEventProperties _properties { get; set; }

        public ServiceMoveDocsCurrent(SPItemEventProperties properties)
        {
            SPFieldUserValue fld_user = new SPFieldUserValue(properties.Web, properties.ListItem[SPBuiltInFieldId.Author].ToString());
            _User = fld_user.User;
            _tipoConteudo = tipoConteudoGetFolder(properties.ListItem[SPBuiltInFieldId.ContentType].ToString());
            _Web = properties.Web;
            _IdItem = properties.ListItemId;
            _NomeLista = properties.ListTitle;
            _WebConfiguracao = properties.Web.Webs["configuracoes"];
            _properties = properties;
        }

        public void UpdateDocumento()
        {
            DataTable tableGrupos = ObterGrupos();

            if (tableGrupos != null)
            {
                using (SPSite ImpersonatedSite = new SPSite(_Web.Url))
                {
                    using (SPWeb ImpersonatedWeb = ImpersonatedSite.OpenWeb())
                    {
                        string url_destino = ObterEmpresa(tableGrupos.Rows[0].Field<int>("Empresa"));

                        UpdateDocFile(url_destino);
                    }
                }
            }
        }

        public void UpdateDocFile(string url_destino)
        {
            SPFolder docOrigem = _Web.GetFolder(Constants.NameLibraryLegado.ListSPE);

            string libDestiny = _Web.Site.WebApplication.GetResponseUri(SPUrlZone.Default).AbsoluteUri.ToString() + url_destino;

            _properties.Web.AllowUnsafeUpdates = true;

            using (SPSite destSite = new SPSite(libDestiny))
            {
                using (SPWeb destWeb = destSite.OpenWeb())
                {
                    SPFolder dest = destWeb.GetFolder(_tipoConteudo);

                    SPAttachmentCollection attachments = _properties.ListItem.Attachments;

                    SPFile file = _Web.GetFile(attachments.UrlPrefix + attachments[0]);
                    //_properties.ListItem.Attachments.UrlPrefix + _properties.ListItem.Attachments[0];
                    
                    string nome = Util.ValidaTextField(_properties.ListItem[SPBuiltInFieldId.Title]);

                    try
                    {
                        if (dest.Files[nome].Exists)
                        {
                            if (dest.Files[nome].CheckOutStatus == SPFile.SPCheckOutStatus.None)
                            {
                                dest.Files[nome].CheckOut();
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        //string nome = Path.GetFileNameWithoutExtension(file.Name) + "_" + Zero(_properties.ListItem.ID) + Path.GetExtension(file.Name);    

                        SPFile f = dest.Files.Add(nome, file.OpenBinary(), true);

                        /*SPListItem item = f.Item;
                        UpdateProprieties(item);*/

                        if (f.CheckOutStatus != SPFile.SPCheckOutStatus.None)
                        {
                            f.CheckIn(string.Empty);
                        }

                        //ListSPE(web, file, destWeb.Url + "/" + f.Url);

                        //_properties.ListItem[SPBuiltInFieldId.Title] = nome;
                        //SPFieldUrlValue link = new SPFieldUrlValue(destWeb.Url + "/" + _tipoConteudo + "/" + nome);
                        //_properties.ListItem["Documento"] = link.Url;
                        //_properties.ListItem.Update();

                        DeleteFile(file);

                    }
                    catch (Exception err)
                    {
                        ServicePermissaoEsclusivaMassa.WriteLog(_Web, "Mover Item - Lista: " + dest.DocumentLibrary.Title + " ID do Item: " + file.Item.ID, err.Message);
                    }
                }
            }

            _properties.Web.AllowUnsafeUpdates = false;
        }

        public void DeleteDocumento()
        {
            DataTable tableGrupos = ObterGrupos();

            if (tableGrupos != null)
            {
                using (SPSite ImpersonatedSite = new SPSite(_Web.Url))
                {
                    using (SPWeb ImpersonatedWeb = ImpersonatedSite.OpenWeb())
                    {
                        string url_destino = ObterEmpresa(tableGrupos.Rows[0].Field<int>("Empresa"));

                        DeleteDocFile(url_destino);
                    }
                }
            }
        }

        private void DeleteDocFile(string url_destino)
        {
            string libDestiny = _Web.Site.WebApplication.GetResponseUri(SPUrlZone.Default).AbsoluteUri.ToString() + url_destino;

            using (SPSite destSite = new SPSite(libDestiny))
            {
                using (SPWeb destWeb = destSite.OpenWeb())
                {
                    SPFolder dest = destWeb.GetFolder(_tipoConteudo);

                    //SPFile file = destWeb.GetFile(libDestiny + "/" + Util.ValidaTextField(_properties.ListItem[SPBuiltInFieldId.Title]));
                    dest.Files[Util.ValidaTextField(_properties.ListItem[SPBuiltInFieldId.Title])].Delete();
                }
            }
        }

        public void TranferirDocumento()
        {
            DataTable tableGrupos = ObterGrupos();

            if (tableGrupos != null)
            {
                //SPSecurity.RunWithElevatedPrivileges(delegate()
                //{
                using (SPSite ImpersonatedSite = new SPSite(_Web.Url))
                {
                    using (SPWeb ImpersonatedWeb = ImpersonatedSite.OpenWeb())
                    {
                        string url_destino = ObterEmpresa(tableGrupos.Rows[0].Field<int>("Empresa"));

                        MoveDoc(_Web, url_destino);
                    }
                }
                //});
            }

        }

        private void MoveDoc(SPWeb web, string url_destino)
        {
            SPFolder docOrigem = _Web.GetFolder(Constants.NameLibraryLegado.ListSPE);

            string libDestiny = _Web.Site.WebApplication.GetResponseUri(SPUrlZone.Default).AbsoluteUri.ToString() + url_destino;

            _properties.Web.AllowUnsafeUpdates = true;

            using (SPSite destSite = new SPSite(libDestiny))
            {
                using (SPWeb destWeb = destSite.OpenWeb())
                {
                    SPFolder dest = destWeb.GetFolder(_tipoConteudo);

                    SPAttachmentCollection attachments = _properties.ListItem.Attachments;

                    SPFile file = web.GetFile(attachments.UrlPrefix + attachments[0]);
                    //_properties.ListItem.Attachments.UrlPrefix + _properties.ListItem.Attachments[0];

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

                    try
                    {
                        string nome = Path.GetFileNameWithoutExtension(file.Name.Replace(",", "")) + "_" + Zero(_properties.ListItem.ID) + Path.GetExtension(file.Name.Replace(",", ""));

                        SPFile f = dest.Files.Add(nome, file.OpenBinary(), true);

                        SPListItem item = f.Item;
                        UpdateProprieties(item);

                        if (f.CheckOutStatus != SPFile.SPCheckOutStatus.None)
                        {
                            f.CheckIn(string.Empty);
                        }

                        //ListSPE(web, file, destWeb.Url + "/" + f.Url);

                        _properties.ListItem[SPBuiltInFieldId.Title] = nome;
                        SPFieldUrlValue link = new SPFieldUrlValue(destWeb.Url + "/" + _tipoConteudo + "/" + nome);
                        _properties.ListItem["Documento"] = link.Url;
                        _properties.ListItem.Update();

                        DeleteFile(file);

                    }
                    catch (Exception err)
                    {
                        ServicePermissaoEsclusivaMassa.WriteLog(web, "Mover Item - Lista: " + dest.DocumentLibrary.Title + " ID do Item: " + file.Item.ID, err.Message);
                    }
                }
            }

            _properties.Web.AllowUnsafeUpdates = false;

        }

        private void UpdateProprieties(SPListItem item)
        {
            //item["Tipo de Documento"] = _tipoConteudo;
            item["Tipo de Documento"] = _properties.ListItem[SPBuiltInFieldId.ContentType].ToString();
            //item[SPBuiltInFieldId.Author] = _User;

            item.Update();
        }

        private void DeleteFile(SPFile file)
        {
            file.Delete();
        }

        private static string Zero(int id)
        {
            //int id = item.ListItems.Count;
            if (id < 10)
            {
                return "0" + id.ToString();
            }
            else
            {
                return id.ToString();
            }
        }

        protected DataTable ObterGrupos()
        {
            SPList list = _WebConfiguracao.Lists.TryGetList("Grupo");

            DataTable tableGruposDoUsuario = ObterGrupoUsuario(list);
            DataTable table = null;

            if (tableGruposDoUsuario != null && tableGruposDoUsuario.Rows.Count > 0)
            {
                SPQuery query = new SPQuery();
                query.Query = "<Where><Eq><FieldRef Name='Empresa' LookupId='FALSE'/><Value Type='Lookup' >" + tableGruposDoUsuario.Rows[0].Field<string>("Empresa") + "</Value></Eq></Where>";
                query.ViewFields = "<FieldRef Name='Title' /><FieldRef Name='Perfil' /><FieldRef Name='Empresa' />";

                SPListItemCollectionPosition collPoss;
                table = list.GetDataTable(query, SPListGetDataTableOptions.RetrieveLookupIdsOnly, out collPoss);
            }
            return table;
        }

        protected string ObterEmpresa(int idempresa)
        {
            SPList list = _WebConfiguracao.Lists.TryGetList("Empresa");

            SPListItem item = list.GetItemById(idempresa);

            return Util.ValidaTextField(item["UrlDocumentos"]);
        }

        protected string tipoConteudoGetFolder(string tipoConteudo)
        {
            if (tipoConteudo == "Documentos Plano de Negócio")
            {       
                return "Documentos Plano de Negcio";
            }
            else if (tipoConteudo == "Documentos Balanço Patrimonial")
            {
                return "Documentos Balano Patrimonial";
            }
            else if (tipoConteudo == "Documentos Licença")
            {
                return "Documentos Licena";

            }
            else if (tipoConteudo == "Documentos Remuneração Global")
            {
                return "Documentos Remunerao Global";
            }
            else
            {
                return tipoConteudo;
            }
        }

    }
}

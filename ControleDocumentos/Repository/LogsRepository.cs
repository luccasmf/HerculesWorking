using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ControleDocumentosLibrary;
using ControleDocumentos.Util.Extension;
using ControleDocumentos.Models;

namespace ControleDocumentos.Repository
{
    public class LogsRepository
    {
        DocumentosModel db = new DocumentosModel();
        public bool SalvarLog(Logs log)
        {
            log.Data = DateTime.Now;
            db.Logs.Add(log);

            try
            {
                return db.SaveChanges() > 0;

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public List<Logs> GetByFilter(LogFilter logFilter)
        {
            List<Logs> logs = db.Logs.ToList();

            if(logFilter.Acao != null)
            {
                logs = logs.Where(x => x.Acao == logFilter.Acao).ToList();
            }
            if(logFilter.IdObjeto != null)
            {
                logs = logs.Where(x => x.IdObjeto == logFilter.IdObjeto).ToList();
            }
            if(logFilter.TipoObjeto != null)
            {
                logs = logs.Where(x => x.TipoObjeto == logFilter.TipoObjeto).ToList();
            }
            if(logFilter.Usuario != null)
            {
                logs = logs.Where(x => x.Usuario.Nome.Contains(logFilter.Usuario)).ToList();
            }
           
            return logs;
        }

        public List<Logs> GetLogByUserId(string id)
        {
            return db.Logs.Where(x => x.IdUsuario == id).ToList();
        }

        public List<Logs> GetLogByObj<T>(int idObj, T objeto)
        {
            string nomeObj = objeto.GetType().Name;
            List<Logs> logs = db.Logs.Where(x => x.IdObjeto == idObj && x.TipoObjeto == EnumExtensions.GetValueFromDescription<EnumTipoObjeto>(nomeObj)).ToList();

            return logs;
        }

        public List<Logs> GetLogByData(DateTime data)
        {
            return db.Logs.Where(x => x.Data.ToShortDateString() == data.ToShortDateString()).ToList();
        }
    }
}
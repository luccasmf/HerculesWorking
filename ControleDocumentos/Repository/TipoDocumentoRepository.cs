using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ControleDocumentosLibrary;

namespace ControleDocumentos.Repository
{
    public class TipoDocumentoRepository
    {
        DocumentosModel db = new DocumentosModel();

        /// <summary>
        /// Persisto o novo tipo de documento na base de dados
        /// </summary>
        /// <param name="tipo">nome do novo tipo de documeto</param>
        /// <returns>retorna o tipo cadastrado</returns>
        public bool CadastraTipoDoc(TipoDocumento tipo)
        {
            TipoDocumento tipoDoc = new TipoDocumento();

            if (tipo.IdTipoDoc > 0)
            {
                tipoDoc = db.TipoDocumento.Find(tipo.IdTipoDoc);
                tipoDoc.TipoDocumento1 = tipo.TipoDocumento1;
            }
            else
            {
                db.TipoDocumento.Add(tipo);
            }

            return db.SaveChanges() > 0;
        }

        /// <summary>
        /// Deleta o tipo de documento selecionado da base de dados
        /// </summary>
        /// <param name="id">id do tipo selecionado</param>
        /// <returns>retorna verdadeiro ou falso</returns>
        public bool DeletaTipoDoc(int id)
        {
            TipoDocumento tipo = db.TipoDocumento.Find(id);
            db.TipoDocumento.Remove(tipo);

            return db.SaveChanges() > 0;
        }

        /// <summary>
        /// Busca todos os tipos de documento já cadastrados no sistema
        /// </summary>
        /// <returns>retorna um array de tipos de documento</returns>
        public List<TipoDocumento> listaTipos()
        {
            return db.TipoDocumento.ToList();
        }


        /// <summary>
        /// Retorna o tipo do id buscado
        /// </summary>
        /// <param name="param"></param>
        /// <returns>retorna o tipo de documento desejado</returns>
        public TipoDocumento GetTipoDoc(string nomeTipo)
        {
            if (!string.IsNullOrEmpty(nomeTipo.ToString()))
                return db.TipoDocumento.Where(x => x.TipoDocumento1.ToLower().Contains(nomeTipo.ToLower())).FirstOrDefault();
            return null;
        }

        public List<TipoDocumento> GetTiposByName(string nomeTipo)
        {
            if (!string.IsNullOrEmpty(nomeTipo.ToString()))
                return db.TipoDocumento.Where(x => x.TipoDocumento1.ToLower().Contains(nomeTipo.ToLower())).ToList();
            return null;
        }

        public TipoDocumento GetTipoDocById(int? id)
        {
            if (id.HasValue)
                return db.TipoDocumento.Find(id);
            return null;
        }

        public bool TipoDocExists(TipoDocumento tipo)
        {
            return db.TipoDocumento.Any(t => t.TipoDocumento1 == tipo.TipoDocumento1);
        }
    }
}
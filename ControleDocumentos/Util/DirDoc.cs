using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using ControleDocumentosLibrary;
using System.Security.Cryptography;
using System.Text;
using ControleDocumentos.Repository;
using Novacode;
using ControleDocumentos.Filter;
using Spire.Doc;
using System.Globalization;
using ControleDocumentos.Util;

namespace ControleDocumentos
{
    /// <summary>
    /// Classe responsável por manipular os documentos, separar em pastas e salvar no servidor de arquivos
    /// </summary>
    [AuthorizeAD(Groups = "G_FACULDADE_ALUNOS, G_FACULDADE_PROFESSOR_R, G_FACULDADE_PROFESSOR_RW, G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW, G_FACULDADE_SECRETARIA_R, G_FACULDADE_SECRETARIA_RW")]
    public static class DirDoc
    {
        private static DocumentoRepository documentoRepository = new DocumentoRepository();
        private static TipoDocumentoRepository tipoDocumentoRepository = new TipoDocumentoRepository();
        private static EventoRepository eventoRepository = new EventoRepository();
        private static CursoRepository cursoRepository = new CursoRepository();
        private static UnicodeEncoding ue = new UnicodeEncoding();
        // key for encryption
        static byte[] Key = ue.GetBytes(@"qA!$p(SK");

        //facul
        private static string caminhoBase = @"\\DEVELOPER\Temp\hercules\";

        //casa
        //private static string caminhoBase = @"C:/Hercules/";

        private static string caminhoPadrao = caminhoBase + "Documentos/";
        private static string caminhoTemplates = caminhoBase + "Templates/";
        private static string caminhoDownload = caminhoBase + "Download/";

        public static byte[] converterFileToArray(HttpPostedFileBase x)
        {
            MemoryStream tg = new MemoryStream();
            x.InputStream.CopyTo(tg);
            byte[] data = tg.ToArray();

            return data;
        }

        /// <summary>
        /// Salva o arquivo enviado
        /// </summary>
        /// <param name="doc">documento a ser salvo</param>
        /// <returns>Documento com endereço e nome corretos para inserção no banco de dados</returns>
        public static string SalvaArquivo(Documento doc)
        {
            string curso;
            string idAluno;
            AlunoCurso al;

            try
            {
                al = cursoRepository.GetAlunoCurso(doc.AlunoCurso.IdAluno, doc.AlunoCurso.IdCurso);

            }
            catch
            {
                al = cursoRepository.GetAlunoCurso(doc.IdAlunoCurso);

            }

            try
            {
                curso = cursoRepository.GetCursoById(doc.AlunoCurso.IdCurso).Nome;
                idAluno = doc.AlunoCurso.IdAluno.ToString();
            }
            catch
            {
                curso = cursoRepository.GetCursoById(al.IdCurso).Nome;
                idAluno = al.IdAluno.ToString();
            }


            string tipoDoc = tipoDocumentoRepository.GetTipoDocById(doc.IdTipoDoc).TipoDocumento1;


            List<string> caminho = new List<string>();

            caminho.Add(curso);
            caminho.Add(idAluno);
            caminho.Add(tipoDoc);

            CriaDiretorio(caminho.ToArray());
            doc.NomeDocumento = "(" + al.Aluno.IdUsuario + ")" + GeraNomeArquivo(doc.NomeDocumento);

            string outputFile = CriaDiretorio(caminho.ToArray()) + doc.NomeDocumento;
            doc.CaminhoDocumento = outputFile;

            try
            {
                if (File.Exists(outputFile))
                {
                    return "Arquivo existente";
                }
                else
                {
                    FileStream fs = new FileStream(outputFile, FileMode.Create);
                    RijndaelManaged rmCryp = new RijndaelManaged();
                    CryptoStream cs = new CryptoStream(fs, rmCryp.CreateEncryptor(Key, Key), CryptoStreamMode.Write);

                    foreach (var data in doc.arquivo)
                    {
                        cs.WriteByte((byte)data);
                    }
                    cs.Close();
                    fs.Close();
                }
                doc.Data = DateTime.Now;
                if (documentoRepository.PersisteDocumento(doc))
                {
                    return "Sucesso";
                }

                return "Falha ao persistir";

            }
            catch (Exception e)
            {
                return "Falha ao criptografar";
            }


        }
        /// <summary>
        /// Desencripta arquivo para download
        /// </summary>
        /// <param name="doc">arquivo desejado</param>
        /// <returns>um caminho temporário para baixar o arquivo</returns>
        public static byte[] BaixaArquivo(Documento doc, Usuario user)
        {

            if (user.Permissao == EnumPermissaoUsuario.deslogado)
            {
                return null;
            }

            if (user.Permissao == EnumPermissaoUsuario.aluno)
            {
                if (user.IdUsuario != doc.AlunoCurso.Aluno.IdUsuario)
                {
                    return null;
                }
            }

            try
            {
                //Documento doc = documentoRepository.GetDocumentoByNome(nomeArquivo);
                FileStream fs = new FileStream(doc.CaminhoDocumento, FileMode.Open);
                RijndaelManaged rmCryp = new RijndaelManaged();
                CryptoStream cs = new CryptoStream(fs, rmCryp.CreateDecryptor(Key, Key), CryptoStreamMode.Read);
                string caminho = caminhoDownload + doc.AlunoCurso.IdAluno + doc.NomeDocumento;

                System.IO.Directory.CreateDirectory(caminhoDownload);
                FileStream fsOut = new FileStream(caminho, FileMode.Create);

                //StreamWriter fsDecrypted = new StreamWriter(caminho);
                //fsDecrypted.Write(new StreamReader(cs).ReadToEnd());
                //fsDecrypted.Flush();
                //fsDecrypted.Close();

                int data;

                while ((data = cs.ReadByte()) != -1)
                    fsOut.WriteByte((byte)data);

                fs.Close();
                fsOut.Close();
                cs.Close();

                byte[] arquivo = File.ReadAllBytes(caminho);
                File.Delete(caminho);

                return arquivo;
            }
            catch (Exception e)
            {
                return null;
            }


        }

        /// <summary>
        /// Deleta o arquivo especificado
        /// </summary>
        /// <param name="caminhoArquivo">caminho do arquivo</param>
        /// <returns>retorna true ou false</returns>
        public static bool DeletaArquivo(string caminhoArquivo)
        {
            try
            {
                File.Delete(caminhoArquivo);
                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// Gera certificado para todos os alunos que tiverem 100% de presença no evento
        /// </summary>
        /// <param name="idEvento">evento desejado</param>
        /// <returns></returns>
        public static bool GeraCertificado(int idEvento)
        {
            AlunoRepository ar = new AlunoRepository();
            Evento ev = eventoRepository.GetEventoById(idEvento);
            bool flag = true;
            string templatePath = caminhoTemplates + "template-certificado.docx"; //caminhoDoTemplate
            List<Documento> certificadosDoc = new List<Documento>();
            var template = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            List<Aluno> alunos = eventoRepository.GetAlunosPresentes(ev); //alunos com presença            

            foreach (Aluno al in alunos)
            {
                string curso = al.AlunoCurso.Select(x => x.Curso.Nome).FirstOrDefault().ToString();
                string novoArquivo = CriaDiretorio(new string[] { curso, al.IdAluno.ToString(), "Certificado" });
                novoArquivo = novoArquivo + "(" + al.IdUsuario + ") " + ev.IdEvento + " - " + "temp" + ev.NomeEvento + ".docx";
                Documento documento = new Documento();
                TipoDocumento tipoDoc = tipoDocumentoRepository.GetTipoDoc("Certificado");

                documento.IdTipoDoc = tipoDoc.IdTipoDoc;
                documento.NomeDocumento = "(" + al.IdUsuario + ") " + ev.IdEvento + " - " + "temp" + ev.NomeEvento + ".pdf";
                documento.Data = DateTime.Now;

                documento.IdAlunoCurso = al.AlunoCurso.Select(x => x.IdAlunoCurso).FirstOrDefault();
                documento.IdEvento = ev.IdEvento;

                if (File.Exists(Path.ChangeExtension(novoArquivo, ".pdf")))
                {
                    // certificadosDoc.Add(documento);
                }
                else
                {
                    {

                        #region substitui informaçoes seguindo a template       


                        DocX doc = DocX.Create(novoArquivo);

                        doc.ApplyTemplate(template, true);


                        if (doc.Text.Contains("<NOME>"))
                        {
                            doc.ReplaceText("<NOME>", al.Usuario.Nome);
                        }
                        if (doc.Text.Contains("<EVENTO>"))
                        {
                            doc.ReplaceText("<EVENTO>", ev.NomeEvento);
                        }
                        if (doc.Text.Contains("<HORAS>"))
                        {
                            doc.ReplaceText("<HORAS>", ev.CargaHoraria.ToString());
                        }
                        if (doc.Text.Contains("<DIA>"))
                        {
                            doc.ReplaceText("<DIA>", DateTime.Now.Day.ToString());
                        }
                        if (doc.Text.Contains("<MES>"))
                        {
                            string mes = DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture("pt-BR"));
                            doc.ReplaceText("<MES>", mes);
                        }
                        if (doc.Text.Contains("<ANO>"))
                        {
                            doc.ReplaceText("<ANO>", DateTime.Now.Year.ToString());
                        }

                        doc.Save();
                        doc.Dispose();
                        Document pdf = new Document();
                        pdf.LoadFromFile(novoArquivo);
                        File.Delete(novoArquivo);
                        novoArquivo = Path.ChangeExtension(novoArquivo, ".pdf");
                        pdf.SaveToFile(novoArquivo, FileFormat.PDF);
                    }

                    #region criptografa certificado
                    try
                    {
                        string caminhonovo = novoArquivo.Replace("temp", "");
                        FileStream fs = new FileStream(caminhonovo, FileMode.Create);
                        byte[] arquivo = File.ReadAllBytes(novoArquivo);
                        File.Delete(novoArquivo);
                        RijndaelManaged rmCryp = new RijndaelManaged();
                        CryptoStream cs = new CryptoStream(fs, rmCryp.CreateEncryptor(Key, Key), CryptoStreamMode.Write);

                        foreach (var data in arquivo)
                        {
                            cs.WriteByte((byte)data);
                        }
                        cs.Close();
                        fs.Close();
                        documento.CaminhoDocumento = caminhonovo;
                        certificadosDoc.Add(documento);
                        ar.AdicionaHoras(ev.CargaHoraria, al.IdAluno, ev.IdEvento);
                    }
                    catch (Exception e)
                    {
                        flag = false;
                    }
                    #endregion

                    #endregion
                }

            }

            if (!documentoRepository.PersisteCertificados(certificadosDoc))
            {
                flag = false;
            }
            else
            {
            }

            return flag;

        }

        //private static string GetMes(int month)
        //{
        //    switch(month)
        //    {

        //    }
        //}


        /// <summary>
        /// Verifica a existencia do diretorio, caso não exista, cria
        /// </summary>
        /// <param name="dir">array com a ordem dos diretorios a serem criados</param>
        /// <returns>string com o endereço final</returns>
        private static string CriaDiretorio(string[] dir)
        {
            string pasta = caminhoPadrao;

            for (int i = 0; i < dir.Length; i++)
            {
                pasta += dir[i] + "/";
                if (!Directory.Exists(pasta))
                {
                    //Criamos um com o nome folder
                    Directory.CreateDirectory(pasta);

                }
            }

            return pasta;
        }

        /// <summary>
        /// Pega as informações MIME do arquivo
        /// </summary>
        /// <param name="nomeArquivo">nome do Arquivo como vem do formulario de inserção</param>
        /// <returns>string de informações MIME</returns>
        private static string GetMime(string nomeArquivo)
        {
            return System.Web.MimeMapping.GetMimeMapping(nomeArquivo);
        }

        /// <summary>
        /// Gera novo nome para o arquivo, baseado na data, mantendo formato padrão
        /// </summary>
        /// <param name="nomeAntigo">nome inicial do arquivo</param>
        /// <returns>string com novo nome</returns>
        private static string GeraNomeArquivo(string nomeAntigo)
        {
            string novoNome = "";
            DateTime dt = DateTime.Now;
            string formato = Path.GetExtension(nomeAntigo);

            novoNome = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, formato);

            return novoNome;

        }

    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using FormRecognizer.DTO;

namespace FormRecognizer
{
    public static class ExtractFGTSInfo
    {
        [FunctionName("ExtratoFgts")]
        public static async Task<ExtratoFgtsDTO> ExtratoFgts(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "files")] HttpRequest req,
            ILogger log)
        {
            string endpoint = "https://form-recognizer-fgts.cognitiveservices.azure.com/";
            string key = "404445957ace4de7bf057e972015de7b";
            AzureKeyCredential credential = new AzureKeyCredential(key);
            DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(endpoint), credential);

            Uri fileUri = new Uri("https://fgts.blob.core.windows.net/fgts-documents/testefgts3_page-0001.jpg");

            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-document", fileUri);

            AnalyzeResult result = operation.Value;

            Console.WriteLine("Detected key-value pairs:");

            var extratoFgtsDTO = new ExtratoFgtsDTO();
            foreach (DocumentKeyValuePair kvp in result.KeyValuePairs)
            {
                if (kvp.Value == null)
                {
                    Console.WriteLine($"Found key with no value: '{kvp.Key.Content}'");
                }
                else
                {
                    Console.WriteLine($"Found key-value pair: '{kvp.Key.Content}' and '{kvp.Value.Content}'");

                    switch (kvp.Key.Content)
                    {
                        case "01-RAZÃO SOCIAL/NOME":
                            extratoFgtsDTO.RazaoSocial = kvp.Value.Content;
                            break;
                        case "02-DDD/TELEFONE":
                            extratoFgtsDTO.Telefone = kvp.Value.Content;
                            break;
                        case "10-INSCRIÇÃO/TIPO ( 8)":
                            extratoFgtsDTO.Cnpj = kvp.Value.Content;
                            break;
                        case "05-REMUNERAÇÃO":
                            extratoFgtsDTO.Remuneracao = kvp.Value.Content;
                            break;
                        case "11-COMPETÊNCIA":
                            extratoFgtsDTO.Competencia = kvp.Value.Content;
                            break;
                        case "12-DATA DE VALIDADE":
                            extratoFgtsDTO.DataValidade = kvp.Value.Content;
                            break;
                        case "13-DEPÓSITO + CONTRIB SOCIAL":
                            extratoFgtsDTO.DepositoContribuicaoSindical = kvp.Value.Content;
                            break;
                        case "15-TOTAL A RECOLHER":
                            var totalARecolher = kvp.Value.Content.Split("\n");
                            extratoFgtsDTO.TotalARecolher = totalARecolher.Count() > 1 ? totalARecolher[1] : totalARecolher[0];
                            break;
                        default:
                            break;
                    }
                }
            }

            return extratoFgtsDTO;
        }
    }
}

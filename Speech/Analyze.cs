using Google.Cloud.Language.V1;
using System;
using System.Collections.Generic;
using static Google.Cloud.Language.V1.AnnotateTextRequest.Types;
using Google.Protobuf.Collections;
using Google.Cloud.Translation.V2;

namespace GoogleCloudSamples
{
    public class Analyze
    {
       
            public static string Usage = @"Usage:
C:\> dotnet run command text
C:\> dotnet run command gs://bucketName/objectName
Where command is one of
    entities
    sentiment
    syntax
    entity-sentiment
    classify-text
    everything
";

            // [START analyze_entities_from_file]
            public static void AnalyzeEntitiesFromFile(string gcsUri)
            {
                var client = LanguageServiceClient.Create();
                var response = client.AnalyzeEntities(new Document()
                {
                    GcsContentUri = gcsUri,
                    Type = Document.Types.Type.PlainText
                });
                WriteEntities(response.Entities);
            }
            // [END analyze_entities_from_file]

            // [START analyze_entities_from_string]
            public static void AnalyzeEntitiesFromText(string text)
            {
                var client = LanguageServiceClient.Create();
                var response = client.AnalyzeEntities(new Document()
                {
                    Content = text,
                    Type = Document.Types.Type.PlainText
                });
                WriteEntities(response.Entities);
            }

            // [START analyze_entities_from_file]
            public static void WriteEntities(IEnumerable<Entity> entities)
            {
                Console.WriteLine("Entities:");
                foreach (var entity in entities)
                {
                    Console.WriteLine($"\tName: {entity.Name}");
                    Console.WriteLine($"\tType: {entity.Type}");
                    Console.WriteLine($"\tSalience: {entity.Salience}");
                    Console.WriteLine("\tMentions:");
                    foreach (var mention in entity.Mentions)
                        Console.WriteLine($"\t\t{mention.Text.BeginOffset}: {mention.Text.Content}");
                    Console.WriteLine("\tMetadata:");
                    foreach (var keyval in entity.Metadata)
                    {
                        Console.WriteLine($"\t\t{keyval.Key}: {keyval.Value}");
                    }
                }
            }
            // [END analyze_entities_from_file]
            // [END analyze_entities_from_string]

            // [START analyze_sentiment_from_file]
            public static void AnalyzeSentimentFromFile(string gcsUri)
            {
                var client = LanguageServiceClient.Create();
                var response = client.AnalyzeSentiment(new Document()
                {
                    GcsContentUri = gcsUri,
                    Type = Document.Types.Type.PlainText
                });
                WriteSentiment(response.DocumentSentiment, response.Sentences);
            }
            // [END analyze_sentiment_from_file]

            // [START analyze_sentiment_from_string]
            public static void AnalyzeSentimentFromText(string text)
            {
                var client = LanguageServiceClient.Create();
                var response = client.AnalyzeSentiment(new Document()
                {
                    Content = text,
                    Type = Document.Types.Type.PlainText
                });
                WriteSentiment(response.DocumentSentiment, response.Sentences);
            }

            // [START analyze_sentiment_from_file]
            public static void WriteSentiment(Sentiment sentiment,
                RepeatedField<Sentence> sentences)
            {
                Console.WriteLine("Overall document sentiment:");
                Console.WriteLine($"\tScore: {sentiment.Score}");
                Console.WriteLine($"\tMagnitude: {sentiment.Magnitude}");
                Console.WriteLine("Sentence level sentiment:");
                foreach (var sentence in sentences)
                {
                    Console.WriteLine($"\t{sentence.Text.Content}: "
                        + $"({sentence.Sentiment.Score})");
                }
            }
        // [END analyze_sentiment_from_file]
        // [END analyze_sentiment_from_string]
        static string DetectLanguage(string text)
        {
            TranslationClient client = TranslationClient.Create();
            try
            {
                var detection = client.DetectLanguage(text);
                return detection.Language;
            }catch (Exception e)
            {
                Console.WriteLine("Connection to detect language is failed.");
                return null;
            }
            
        }
        // [START analyze_syntax_from_file]
        public static void AnalyzeSyntaxFromFile(string gcsUri)
            {
                var client = LanguageServiceClient.Create();
                var response = client.AnnotateText(new Document()
                {
                    GcsContentUri = gcsUri,
                    Type = Document.Types.Type.PlainText
                },
                new Features() { ExtractSyntax = true });
                WriteSentences(response.Sentences, response.Tokens);
            }
            // [END analyze_syntax_from_file]

            // [START analyze_syntax_from_string]
            public static RepeatedField<Token> AnalyzeSyntaxFromText(string text)
            {
           


            try
            {
                var client = LanguageServiceClient.Create();
                var response = client.AnnotateText(new Document()


                {
                    Content = TranslateWithModel(text, "en", DetectLanguage(text), TranslationModel.NeuralMachineTranslation),
                    Type = Document.Types.Type.PlainText
                },
                new Features() { ExtractSyntax = true });
                //WriteSentences(response.Sentences, response.Tokens);
                return response.Tokens;

            } catch (Exception e)

            {
                Console.WriteLine("Cannot analyze text.");
                return null;
            }
               
            }

            // [START analyze_syntax_from_file]
            public static void WriteSentences(IEnumerable<Sentence> sentences,
                RepeatedField<Token> tokens)
            {
                Console.WriteLine("Sentences:");
                foreach (var sentence in sentences)
                {
                    Console.WriteLine($"\t{sentence.Text.BeginOffset}: {sentence.Text.Content}");
                }
                Console.WriteLine("Tokens:");
                foreach (var token in tokens)
                {
              
                    Console.WriteLine($"\t{token.PartOfSpeech.Tag} "
                        + $"{token.Text.Content}");
                }
            }
            // [END analyze_syntax_from_file]
            // [END analyze_syntax_from_string]

            // [START analyze_entity_sentiment_from_file]
            public static void AnalyzeEntitySentimentFromFile(string gcsUri)
            {
                var client = LanguageServiceClient.Create();
                var response = client.AnalyzeEntitySentiment(new Document()
                {
                    GcsContentUri = gcsUri,
                    Type = Document.Types.Type.PlainText
                });
                WriteEntitySentiment(response.Entities);
            }
            // [END analyze_entity_sentiment_from_file]

            // [START analyze_entity_sentiment_from_string]
            public static void AnalyzeEntitySentimentFromText(string text)
            {
                var client = LanguageServiceClient.Create();
                var response = client.AnalyzeEntitySentiment(new Document()
                {
                    Content = text,
                    Type = Document.Types.Type.PlainText
                });
                WriteEntitySentiment(response.Entities);
            }

            // [START analyze_entity_sentiment_from_file]
            public static void WriteEntitySentiment(IEnumerable<Entity> entities)
            {
                Console.WriteLine("Entity Sentiment:");
                foreach (var entity in entities)
                {
                    Console.WriteLine($"\t{entity.Name} "
                        + $"({(int)(entity.Salience * 100)}%)");
                    Console.WriteLine($"\t\tScore: {entity.Sentiment.Score}");
                    Console.WriteLine($"\t\tMagnitude { entity.Sentiment.Magnitude}");
                }
            }
            // [END analyze_entity_sentiment_from_file]
            // [END analyze_entity_sentiment_from_string]

            // [START language_classify_file]
            public static void ClassifyTextFromFile(string gcsUri)
            {
                var client = LanguageServiceClient.Create();
                var response = client.ClassifyText(new Document()
                {
                    GcsContentUri = gcsUri,
                    Type = Document.Types.Type.PlainText
                });
                WriteCategories(response.Categories);
            }
            // [END language_classify_file]

            // [START language_classify_string]
            public static void ClassifyTextFromText(string text)
            {
                var client = LanguageServiceClient.Create();
                var response = client.ClassifyText(new Document()
                {
                    Content = text,
                    Type = Document.Types.Type.PlainText
                });
                WriteCategories(response.Categories);
            }

            // [START language_classify_file]
            public static void WriteCategories(IEnumerable<ClassificationCategory> categories)
            {
                Console.WriteLine("Categories:");
                foreach (var category in categories)
                {
                    Console.WriteLine($"\tCategory: {category.Name}");
                    Console.WriteLine($"\t\tConfidence: {category.Confidence}");
                }
            }
            // [END language_classify_string]
            // [END language_classify_file]

            public static void AnalyzeEverything(string text)
            {
                var client = LanguageServiceClient.Create();
                var response = client.AnnotateText(new Document()
                {
                    Content = text,
                    Type = Document.Types.Type.PlainText
                },
                new Features()
                {
                    ExtractSyntax = true,
                    ExtractDocumentSentiment = true,
                    ExtractEntities = true,
                    ExtractEntitySentiment = true,
                    ClassifyText = true,
                });
                Console.WriteLine($"Language: {response.Language}");
                WriteSentiment(response.DocumentSentiment, response.Sentences);
                WriteSentences(response.Sentences, response.Tokens);
                WriteEntities(response.Entities);
                WriteEntitySentiment(response.Entities);
                WriteCategories(response.Categories);
            }
        static String Translate(string text, string targetLanguageCode,
        string sourceLanguageCode)
            {
                TranslationClient client = TranslationClient.Create();
                var response = client.TranslateText(text, targetLanguageCode,
                    sourceLanguageCode);
                return response.TranslatedText;
            }
        static String TranslateWithModel(string text,
        string targetLanguageCode, string sourceLanguageCode,
        TranslationModel model)
            {
                TranslationClient client = TranslationClient.Create();
            try
            {
                var response = client.TranslateText(text,targetLanguageCode, sourceLanguageCode, model);
                // Console.WriteLine("Model: {0}", response.Model);

                return response.TranslatedText;
            }
            catch (Exception e)
            {
                Console.WriteLine("Connection to translate failed.");
                return null;
            }


            }
        public static void Main(string[] args)
            {
                if (args.Length < 2)
                {
                    Console.Write(Usage);
                Console.ReadLine();
                return;
                }
                string command = args[0].ToLower();
                string text = string.Join(" ",
                    new ArraySegment<string>(args, 1, args.Length - 1));

                string gcsUri = args[1].ToLower().StartsWith("gs://") ? args[1] : null;
                switch (command)
                {
                    case "entities":
                        if (null == gcsUri)
                            AnalyzeEntitiesFromText(text);
                        else
                            AnalyzeEntitiesFromFile(gcsUri);
                    Console.ReadLine();

                    break;

                    case "syntax":
                        if (null == gcsUri)
                            AnalyzeSyntaxFromText(text);
                        else
                            AnalyzeSyntaxFromFile(gcsUri);
                        break;

                    case "sentiment":
                        if (null == gcsUri)
                            AnalyzeSentimentFromText(text);
                        else
                            AnalyzeSentimentFromFile(gcsUri);
                        break;

                    case "entity-sentiment":
                        if (null == gcsUri)
                            AnalyzeEntitySentimentFromText(text);
                        else
                            AnalyzeEntitySentimentFromFile(gcsUri);
                        break;

                    case "classify-text":
                        if (null == gcsUri)
                            ClassifyTextFromText(text);
                        else
                            ClassifyTextFromFile(gcsUri);
                        break;

                    case "everything":
                        AnalyzeEverything(text);
                        break;

                    default:
                        Console.Write(Usage);
                    Console.ReadLine();
                    return;
                }
            Console.ReadLine();


        }
        }
    }

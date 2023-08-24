using Microsoft.Extensions.Configuration;
using Azure;
using Azure.AI.TextAnalytics;
using Azure.AI.Language.QuestionAnswering;
using System;
using System.Collections.Generic;
using System.IO;

namespace Lab1QnAnNLP
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Load configuration settings from appsettings.json
            IConfiguration configuration = LoadConfiguration();

            // Initialize clients for QnA Maker and Text Analytics services
            var qnaClient = InitializeQnAClient(configuration);
            var textAnalyticsClient = InitializeTextAnalyticsClient(configuration);

            var random = new Random();

            while (true)
            {
                Console.WriteLine("Enter your question (or 'quit' to exit):");
                string userInput = Console.ReadLine();

                if (userInput.ToLower() == "quit")
                {
                    break; // Exit the loop if the user enters "quit"
                }

                // Retrieve answers from QnA Maker service for user's question
                Response<AnswersResult> qnaResponse = GetQnAAnswers(qnaClient, userInput);

                // Print QnA Maker answers
                PrintQnAAnswers(userInput, qnaResponse.Value.Answers);

                // Analyze sentiment of user's input using Text Analytics service
                DocumentSentiment sentimentResult = AnalyzeSentiment(textAnalyticsClient, userInput);
                Console.WriteLine($"Sentiment: {sentimentResult.Sentiment}");
                Console.WriteLine();

                // Print a random humorous reaction
                PrintRandomHumorousReaction(random);
            }
        }

        // Load configuration settings from appsettings.json file
        static IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }

        // Initialize the QnA Maker client using configuration settings
        static QuestionAnsweringClient InitializeQnAClient(IConfiguration configuration)
        {
            var qnaConfig = configuration.GetSection("QnAMakerConfig");
            var qnaCredential = new AzureKeyCredential(qnaConfig["Key"]);
            return new QuestionAnsweringClient(new Uri(qnaConfig["Endpoint"]), qnaCredential);
        }

        // Initialize the Text Analytics client using configuration settings
        static TextAnalyticsClient InitializeTextAnalyticsClient(IConfiguration configuration)
        {
            var textAnalyticsConfig = configuration.GetSection("TextAnalyticsConfig");
            var textAnalyticsCredential = new AzureKeyCredential(textAnalyticsConfig["Key"]);
            return new TextAnalyticsClient(new Uri(textAnalyticsConfig["Endpoint"]), textAnalyticsCredential);
        }

        // Retrieve answers from QnA Maker service for a given question
        static Response<AnswersResult> GetQnAAnswers(QuestionAnsweringClient qnaClient, string question)
        {
            var qnaProject = new QuestionAnsweringProject("CsgoFAQ", "production");
            return qnaClient.GetAnswers(question, qnaProject);
        }

        // Print QnA Maker answers
        static void PrintQnAAnswers(string question, IReadOnlyList<KnowledgeBaseAnswer> answers)
        {
            foreach (var answer in answers)
            {
                Console.WriteLine($"\nQ: {question}");
                Console.WriteLine($"A: {answer.Answer}\n");
            }
        }

        // Analyze sentiment of a text using the Text Analytics service
        static DocumentSentiment AnalyzeSentiment(TextAnalyticsClient textAnalyticsClient, string text)
        {
            return textAnalyticsClient.AnalyzeSentiment(text);
        }

        // Print a random humorous reaction
        static void PrintRandomHumorousReaction(Random random)
        {
            string[] reactions = {
                "Wow, that's quite the question you've got there!",
                "I'm scratching my virtual head over that one!",
                "You've stumped me with your brilliance!",
                "Your question is like a riddle wrapped in an enigma!",
                "Hmm, I'm not sure whether to laugh or cry at that question!"
            };

            int randomIndex = random.Next(reactions.Length);
            Console.WriteLine(reactions[randomIndex] + "\n");
        }
    }
}

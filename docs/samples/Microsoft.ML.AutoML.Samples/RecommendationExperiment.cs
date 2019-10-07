﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using Microsoft.ML.AutoML.Samples.DataStructures;
using Microsoft.ML.Data;

namespace Microsoft.ML.AutoML.Samples
{
    public static class RecommendationExperiment
    {
        private static string TrainDataPath = "<Path to your train dataset goes here>";
        private static string TestDataPath = "<Path to your test dataset goes here>";
        private static string ModelPath = @"<Desired model output directory goes here>\Model.zip";
        private static string LabelColumnName = "Rating";
        private static uint ExperimentTime = 60;

        public static void Run()
        {
            MLContext mlContext = new MLContext();

            // STEP 1: Load data
            IDataView trainDataView = mlContext.Data.LoadFromTextFile<Movie>(TrainDataPath, hasHeader: true, separatorChar: ',');
            IDataView testDataView = mlContext.Data.LoadFromTextFile<Movie>(TestDataPath, hasHeader: true, separatorChar: ',');

            var settings = new RecommendationExperimentSettings(RecommendationExperimentScenario.MatrixFactorization, "UserId", "MovieId")
            {
                MaxExperimentTimeInSeconds = ExperimentTime
            };
            var inputColumnInformation = new ColumnInformation();
            inputColumnInformation.LabelCategoricalColumnNames.Add("MovieId");
            inputColumnInformation.LabelCategoricalColumnNames.Add("UserId");
            inputColumnInformation.LabelColumnName = "Rating";

            // STEP 2: Run AutoML experiment
            Console.WriteLine($"Running AutoML recommendation experiment for {ExperimentTime} seconds...");
            ExperimentResult<RegressionMetrics> experimentResult = mlContext.Auto()
                .CreateRecommendationExperiment(settings)
                .Execute(trainDataView, inputColumnInformation);

            // STEP 3: Print metric from best model
            RunDetail<RegressionMetrics> bestRun = experimentResult.BestRun;
            Console.WriteLine($"Total models produced: {experimentResult.RunDetails.Count()}");
            Console.WriteLine($"Best model's trainer: {bestRun.TrainerName}");
            Console.WriteLine($"Metrics of best model from validation data --");
            PrintMetrics(bestRun.ValidationMetrics);

            // STEP 5: Evaluate test data
            IDataView testDataViewWithBestScore = bestRun.Model.Transform(testDataView);
            RegressionMetrics testMetrics = mlContext.Regression.Evaluate(testDataViewWithBestScore, labelColumnName: LabelColumnName);
            Console.WriteLine($"Metrics of best model on test data --");
            PrintMetrics(testMetrics);

            // STEP 6: Save the best model for later deployment and inferencing
            using (FileStream fs = File.Create(ModelPath))
                mlContext.Model.Save(bestRun.Model, trainDataView.Schema, fs);

            // STEP 7: Create prediction engine from the best trained model
            var predictionEngine = mlContext.Model.CreatePredictionEngine<Movie, MovieRatingPrediction>(bestRun.Model);

            // STEP 8: Initialize a new test, and get the predicted fare
            var testMovie = new Movie
            {
                UserId="1",
                MovieId = "1097",
            };
            var prediction = predictionEngine.Predict(testMovie);
            Console.WriteLine($"Predicted rating for: {prediction.Rating}");

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static void PrintMetrics(RegressionMetrics metrics)
        {
            Console.WriteLine($"MeanAbsoluteError: {metrics.MeanAbsoluteError}");
            Console.WriteLine($"MeanSquaredError: {metrics.MeanSquaredError}");
            Console.WriteLine($"RootMeanSquaredError: {metrics.RootMeanSquaredError}");
            Console.WriteLine($"RSquared: {metrics.RSquared}");
        }
    }
}

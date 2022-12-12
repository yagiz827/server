using System.Collections.Generic;
using System.IO;

namespace server
{
    public class QuizItem
    {
        public string Question { get; set; }
        public int Answer { get; set; }
    }
    public class Quiz
    {
        private List<QuizItem> QuizItems;
        private int NumberOfQuestionsAsked;
        private int NumberOfQuestionsToBeAsked;

        private int TotalNumberOfQuizItems;
        public Quiz(string fileName, int numberOfQuestionsToBeAsked)
        {
            QuizItems = new List<QuizItem>();
            NumberOfQuestionsAsked = 0;
            NumberOfQuestionsToBeAsked = numberOfQuestionsToBeAsked;
            ReadQuestionsFromFile(fileName);
        }
        private void ReadQuestionsFromFile(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);
            for (int i = 0; i < lines.Length; i += 2)
            {
                var question = lines[i];
                var answer = int.Parse(lines[i + 1]);
                var quizItem = new QuizItem()
                {
                    Question = question,
                    Answer = answer
                };
                QuizItems.Add(quizItem);
            }
            TotalNumberOfQuizItems = QuizItems.Count;
        }

        public string GetQuestion()
        {
            var quizItem = QuizItems[NumberOfQuestionsAsked % TotalNumberOfQuizItems];
            return quizItem.Question;
        }
        public int GetAnswer()
        {
            var quizItem = QuizItems[NumberOfQuestionsAsked % TotalNumberOfQuizItems];
            NumberOfQuestionsAsked++;
            return quizItem.Answer;
        }

        public bool IsQuizFinished()
        {
            return NumberOfQuestionsToBeAsked == NumberOfQuestionsAsked;
        }

        public void ResetQuiz(int NumberofQues)
        {
            NumberOfQuestionsAsked = 0;
            NumberOfQuestionsToBeAsked = NumberofQues;
        }
    }
}

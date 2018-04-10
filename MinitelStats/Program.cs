using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MinitelStats
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            String basePath = "/Users/sfardoux/Perso/2018 JEMA";

            QCM qcm = new QCM();
            String fileStatsByHour = basePath + "statsByHour.csv";
            List<String> statsByHour = new List<string>();
            String fileScores = basePath + "scores.csv";
            Dictionary<string, Gamer> scores = new Dictionary<string, Gamer>();
            String fileQuestions = basePath + "questions.csv";
            Dictionary<string, Questions> questions = new Dictionary<string, Questions>();
            String filePoints = basePath + "points.csv";
            Dictionary<string, int> allPoints = new Dictionary<string, int>();
            int cpt = 0;
            foreach (string file in Directory.GetFiles(basePath, "G*.TXT", SearchOption.AllDirectories))
            {
                cpt++;
                int posPoint = file.LastIndexOf(".");
                int posStart = file.LastIndexOf("\\");
                if (posStart < 0)
                    posStart = file.LastIndexOf("/");
                string name = file.Substring(posStart + 1, posPoint - posStart - 1);

                Console.Write("Working on file: {0}", name);
                String data = File.ReadAllText(file);
                String[] parts = data.Split('|');
                int score = Convert.ToInt32(parts[2]);
                if (score > 300)
                {
                    Console.WriteLine(string.Format(" ignored because score = {0}", score));
                    continue;
                }
                // datetime | nom | score | [id question | good/false ]
                Console.WriteLine(" @ {0} {1}: {2}", parts[0], parts[1], parts[2]);
                statsByHour.Add(string.Format("{0}:{1}:{2};1",parts[0].Substring(8, 2), parts[0].Substring(10, 2), parts[0].Substring(12, 2)));
                if(scores.ContainsKey(parts[1]))
                {
                    scores[parts[1]].scores.Add(score);
                } else {
                    Gamer g = new Gamer();
                    g.name = parts[1];
                    g.scores = new List<int>();
                    g.scores.Add(score);
                    scores.Add(g.name, g);
                }
                for (int i = 3; i < parts.Length; i+=2)
                {
                    if(questions.ContainsKey(parts[i]))
                    {
                        questions[parts[i]].Number++;
                    } else {
                        Questions q = new Questions();
                        q.ID = parts[i];
                        q.Number = 1;
                        q.Bad = 0;
                        q.Good = 0;
                        questions.Add(parts[i], q);
                    }
                    if (parts[i + 1] == "0")
                        questions[parts[i]].Bad++;
                    else
                        questions[parts[i]].Good++;
                }
                if(allPoints.ContainsKey(parts[2]))
                {
                    allPoints[parts[2]]++;
                } else {
                    allPoints.Add(parts[2], 1);
                }
            }
            Console.WriteLine(string.Format("Nb de jeux: {0}", cpt));
            var orderScores = scores.OrderBy(x => x.Key).ToList();
            StringBuilder sb = new StringBuilder();
            foreach(var x in orderScores)
            {
                foreach (var s in x.Value.scores)
                {
                    sb.AppendLine(String.Format("{0};{1}", x.Key.Replace(";", ","), s));

                }
            }
            File.WriteAllText(fileScores, sb.ToString());
            sb = new StringBuilder();
            foreach(string uuid in qcm.UUID)
            {
                if(!questions.ContainsKey(uuid))
                {
                    Questions q = new Questions();
                    q.ID = uuid;
                    q.Number = 0;
                    q.Bad = 0;
                    q.Good = 0;
                    questions.Add(uuid, q);
                }
            }
            sb.AppendLine("UUID;Number;Good;False;Level;New level;Change;Question");
            foreach (var x in questions)
            {
                int index = 0;
                while (qcm.UUID[index] != x.Key)
                    index++;
                int level = qcm.Level[index];
                String change = "";
                if (x.Value.Number == x.Value.Good)
                {
                    level--;
                    change = "-";
                }
                if (x.Value.Number == x.Value.Bad)
                {
                    level++;
                    change = "+";
                }
                sb.AppendLine(String.Format("{0};{1};{2};{3};{4};{5};{6};{7}", x.Key, x.Value.Number, x.Value.Good, x.Value.Bad, qcm.Level[index], level, change, qcm.Questions[index]));
            }
            File.WriteAllText(fileQuestions, sb.ToString(), Encoding.UTF8);
            statsByHour.Sort();
            File.WriteAllLines(fileStatsByHour, statsByHour);
            sb = new StringBuilder();
            foreach (var p in allPoints)
            {
                sb.AppendLine(string.Format("{0};{1}", p.Key, p.Value));
            }
            File.WriteAllText(filePoints, sb.ToString());


        }
    }
}

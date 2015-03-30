using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AppStore.Models;

namespace AppStore.Controllers
{
    public class RecomendationController : Controller
    {
        readonly AppStoreDbEntities _db = new AppStoreDbEntities();

        // GET: Recomendation
        public ActionResult Index()
        {

            List<Likness> liknesses = new List<Likness>();
            List<Users> users = new List<Users>();

            foreach (Users user in _db.Users)
            {
                foreach (Users otherUser in _db.Users)
                {
                    if (user.Name == otherUser.Name)
                    {
                        liknesses.Add(new Likness() {First = user, Second = otherUser, LiknessScore = 0});
                    }
                    else
                    {
                        List<Apps> togetherApps = new List<Apps>();

                        foreach (Scores userScore in user.Scores)
                        {
                            foreach (Scores otherScores in otherUser.Scores)
                            {
                                if (userScore.Apps == otherScores.Apps)
                                {
                                    togetherApps.Add(userScore.Apps);
                                }
                            }
                        }

                        double summ = 0;
                        foreach (Scores userScore in user.Scores)
                        {
                            foreach (Scores otherScores in otherUser.Scores)
                            {
                                if (togetherApps.Contains(userScore.Apps) && togetherApps.Contains(otherScores.Apps))
                                {
                                    double fufa = userScore.Score;
                                    double fusa =
                                        (from score in user.Scores
                                            where score.Apps == otherScores.Apps
                                            select score.Score)
                                            .FirstOrDefault();

                                    double sufa =
                                        (from score in otherUser.Scores
                                            where score.Apps == otherScores.Apps
                                            select score.Score)
                                            .FirstOrDefault();
                                    double susa = otherScores.Score;
                                    summ += Math.Abs((fufa - sufa) + (fusa - susa));
                                }
                            }
                        }
                        double liknessScore = 0;

                        if (summ != 0.0)
                            liknessScore = 1.0/Math.Sqrt(summ);

                        liknesses.Add(new Likness() {First = user, Second = otherUser, LiknessScore = liknessScore});
                    }
                }

               users.Add(user);
            }



            List<PersonalReccomendation> personalReccomendation = new List<PersonalReccomendation>();
            if (User.Identity.IsAuthenticated)
            {
                Users user = _db.Users.SingleOrDefault(x => x.Name == User.Identity.Name);
                if (user != null)
                {
                    List<Likness> usersLikeMe = new List<Likness>();
                    double maxLikness = 0;
                    foreach (Likness likness in liknesses)
                    {
                        if (likness.First == user)
                        {
                            usersLikeMe.Add(likness);
                            if (likness.LiknessScore > maxLikness)
                            {
                                maxLikness = likness.LiknessScore;
                            }
                        }
                    }

                    usersLikeMe =
                        usersLikeMe.OrderByDescending(x => x.LiknessScore)
                            .Where(x => (x.LiknessScore >= (0.75*maxLikness)))
                            .ToList();

                    List<Apps> userApps = (from scores in user.Scores select scores.Apps).ToList();

                    foreach (Likness likness in usersLikeMe)
                    {
                        List<Apps> otherUserApps = (from scores in likness.Second.Scores where scores.Score>4 select scores.Apps).ToList();

                        foreach (Apps apps in otherUserApps)
                        {
                            if (!userApps.Contains(apps))
                            {
                                personalReccomendation.Add(new PersonalReccomendation(){App = apps, User = likness.Second});
                            }
                        }
                    }
                }
            }


            List<BestGame> bestGames = new List<BestGame>();
            List<Apps> appses = _db.Apps.ToList();
            foreach (Apps apps in appses)
            {
                var app = new BestGame() {Apps = apps};
                double avg = 0;
                foreach (Scores scores in apps.Scores)
                {
                    avg += scores.Score;
                }
                avg = avg/apps.Scores.Count;
                app.AvgScore = avg;


                bestGames.Add(app);
            }

            var bestOfThebest = bestGames.OrderByDescending(x => x.AvgScore).ToList();
            if (bestGames.Count > 5)
            {
                bestOfThebest = bestOfThebest.Take(5).ToList();
            }


            RecomendationModel model = new RecomendationModel();
            model.Liknesses = liknesses;
            model.Users = users;
            model.BestGames = bestOfThebest;
            model.PersonalRecomendation = personalReccomendation;

            return View(model);
        }
    }


    public class RecomendationModel
    {
        public List<Likness> Liknesses { get; set; }
        public List<Users> Users { get; set; }
        public List<PersonalReccomendation> PersonalRecomendation { get; set; }
        public List<BestGame> BestGames { get; set; }
    }

    public class Likness
    {
        public Users First { get; set; }
        public Users Second { get; set; }
        public double LiknessScore{ get; set; }
    }

    class Together
    {
        public Apps App { get; set; }
        public Scores First { get; set; }
        public Scores Second { get; set; }
    }

    public class BestGame
    {
        public Apps Apps { get; set; }
        public double AvgScore { get; set; }
    }

    public class PersonalReccomendation
    {
        public Users User { get; set; }
        public Apps App { get; set; }
    }
}
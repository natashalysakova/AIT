using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AppStore.Models;

namespace AppStore.Controllers
{
    public class ScoresController : Controller
    {
        private AppStoreDbEntities db = new AppStoreDbEntities();

        // GET: Scores
        public async Task<ActionResult> Index()
        {
            var scores = db.Scores.Include(s => s.Apps).Include(s => s.Users);
            return View(await scores.ToListAsync());
        }


        // GET: Scores/Create
        public ActionResult Create()
        {
            ViewBag.AppId = new SelectList(db.Apps, "Id", "Title");
            ViewBag.UserId = new SelectList(db.Users, "Id", "Name");
            return View();
        }

        // POST: Scores/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,UserId,AppId,Score")] Scores scores)
        {
            if (ModelState.IsValid)
            {
                db.Scores.Add(scores);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.AppId = new SelectList(db.Apps, "Id", "Title", scores.AppId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "Name", scores.UserId);
            return View(scores);
        }



        public async Task<ActionResult> GenerateRandomScores()
        {
            Random r = new Random();

            List<int> userIds = (from user in db.Users select user.Id).ToList();
            List<int> appsIds = (from app in db.Apps select app.Id).ToList();

            for (int i = 0; i < 20; i++)
            {
                int userId = userIds[r.Next(0, userIds.Count)];
                int appId = appsIds[r.Next(0, appsIds.Count)];

                Users user = await  db.Users.FindAsync(userId);
                if (user != null)
                {
                    bool isExist = (from score in user.Scores where score.AppId == appId select true).FirstOrDefault();

                    if (!isExist)
                    {
                        user.Scores.Add(new Scores(){AppId =  appId, Users = user, Score = r.Next(1,6)});
                    }
                }
            }

            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public async Task<ActionResult> RemoveAllScores()
        {
            var scores = db.Scores.ToList();
            db.Scores.RemoveRange(scores);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}

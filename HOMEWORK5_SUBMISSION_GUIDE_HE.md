# הוראות אחרונות להגשת מטלה 5 — Shadow Switch

## מה כבר הוכן

- `dramatic-elements.md` — **בשורש המאגר**: תשובות מלאות לכל 10 הרכיבים הדרמטיים, מעודכנות לקמפיין הפעיל.
- `dramatic-assets/dramatic-arc.svg` + `.png` — גרף הקשת הדרמטית.
- `story/ShadowSwitch_Story.twee` — סיפור אינטראקטיבי מקורי המיועד ל־Twine (Harlowe), עם בחירות ותנאים.
- `story/ShadowSwitch_story_preview.html` — תצוגת HTML מקומית עצמאית שניתן לפתוח בדפדפן, **לא ייצוא Twine הרשמי**.
- `story/story-map.md` — מפת ההסתעפויות.
- `liveops/eclipse-night.submission-snapshot.json` — צילום תיעודי של קובץ האירוע המרוחק, כשהוא כבוי (`enabled:false`); זה **לא** קובץ הפעולה.
- `README_HOMEWORK5.md` — דף תוכן וקישור אל המשחק המפורסם.

## העלאה ל־GitHub בלי לפגוע במטלה 4

1. פתחו https://github.com/pojectgame12/HomeWork4 בענף `main`.
2. לחצו `Add file` ואז `Upload files`.
3. פתחו את התיקייה `ShadowSwitch_Homework5_Submission` במחשב, **סמנו את כל הפריטים שבתוכה** וגררו אותם למסך ההעלאה: `dramatic-elements.md`, `dramatic-assets`, `story`, `README_HOMEWORK5.md`, `HOMEWORK5_SUBMISSION_GUIDE_HE.md`.
4. התיקייה `liveops` בחבילה מכילה רק **snapshot** בשם נפרד ולכן **לא מחליפה** את הקובץ הפעיל `liveops/eclipse-night.json`. אפשר להעלות גם את התיקייה הזאת; היא תתווסף כתיעוד בלבד.
5. לחצו `Commit changes`. ודאו שכתובת `https://github.com/pojectgame12/HomeWork4/blob/main/dramatic-elements.md` מציגה את המסמך והגרף.
6. אין להעתיק מסמכים אל `Assets/Materials`, אין להחליף קוד Unity ואין צורך להריץ שוב את מתקין הקמפיין.

## ייצוא HTML רשמי מתוך Twine — עדיין נדרש

1. פתחו https://twinery.org/ או את יישום Twine 2 המותקן.
2. ייבאו את `story/ShadowSwitch_Story.twee` דרך Import From File (או השתמשו בכלי ייבוא Twee 3 מתאים לגרסת Twine).
3. הגדירו Harlowe 3.x אם הפורמט אינו נבחר אוטומטית. בדקו את בחירות הסיפור: יומן בטיחות, עזרה למאיה, הפעלה מהירה/בטוחה, נעילה מבוקרת.
4. בחרו `Publish to File`, שמרו בשם `ShadowSwitch_Twine_Published.html` בתיקייה `story/` והעלו גם אותו ל־GitHub.
5. פתחו את ה־HTML שנוצר בדפדפן ובדקו שאפשר להגיע ליותר מסיום אחד. **לא לשנות את שם קובץ ה־preview ולהציגו כייצוא של Twine**.

## בדיקה סופית ב־itch.io

קישור המשחק: https://projectgame12.itch.io/homework5

- פתחו בחלון גלישה בסתר ובדקו טעינה מלאה של Unity WebGL.
- בחרו `SHADOW SWITCH: STORY` מתוך תפריט המשחק, אם הוא נפתח ל־MainMenu.
- בדקו פתיחה/ניצחון לכל שלושת השלבים ומסך סיום; ודאו ששמירה שרדה רענון דף.
- בדקו באירוע מרוחק שה־Source הוא `Remote GitHub JSON`, לא `Local StreamingAssets`.
- הודעה על הצלחה ב־Unity Editor בלבד אינה מחליפה אימות בדפדפן.

## מצב שניתן לציין למרצה

המשחק פורסם ב־itch.io; שלושת השלבים עברו 15 בדיקות ב־Unity; עדכון Live Ops מ־GitHub הופעל וכובה ללא Build חדש בתוך Unity. בדיקת Live Ops בתוך גרסת WebGL ו־HTML רשמי של Twine עדיין דורשות אישור נפרד. אם המרצה דרש אישור מראש לבחירת המשחק, יש לוודא שניתן.

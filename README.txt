Логіни та паролі
Користувач:
test@gmail.com
1234

Адміністратор:
admin@aipharma.com
admin123

Менеджер
manager1@aipharma.com:
manager123

AIPharma — інструкція для запуску проєкту

1. Склад архіву

У переданому архіві мають бути такі основні файли та папки:

- AIPharma_publish — опублікована версія веб-застосунку;
- AIPharma.exe — файл запуску сайту;
- run.bat — файл для швидкого запуску сайту;
- appsettings.json — файл налаштувань підключення до бази даних та AI-модуля;
- aipharma_db_final.sql або aipharma_db.sql — дамп бази даних MySQL;
- вихідний код проєкту, якщо він доданий окремо.

ВАЖЛИВО: архів потрібно спочатку повністю розпакувати. Не слід запускати run.bat або AIPharma.exe безпосередньо з архіву.


2. Необхідне програмне забезпечення

Для запуску проєкту потрібно:

1. MySQL Server 8.0 або новіший.
2. MySQL Workbench для імпорту бази даних.
3. Для опублікованої self-contained версії .NET Runtime зазвичай не потрібен, оскільки він входить до папки публікації.


3. Імпорт бази даних

Перед запуском сайту потрібно імпортувати базу даних.

Порядок дій у MySQL Workbench:

1. Відкрити MySQL Workbench.
2. Підключитися до локального сервера MySQL, наприклад Local instance MySQL80.
3. Обрати меню:

   Server -> Data Import

4. Вибрати пункт:

   Import from Self-Contained File

5. Обрати файл бази даних:

   aipharma_db_final.sql

   або, якщо файл має іншу назву:

   aipharma_db.sql

6. Натиснути Start Import.

Після імпорту повинна з'явитися база даних:

   aipharma_db

Для перевірки можна виконати SQL-запити:

USE aipharma_db;

SELECT COUNT(*) AS users_count FROM users;
SELECT COUNT(*) AS products_count FROM products;
SELECT COUNT(*) AS pharmacies_count FROM pharmacies;
SELECT COUNT(*) AS products_with_images
FROM products
WHERE ImagePath IS NOT NULL AND ImagePath <> '';

Очікувано:

- users_count — більше 0;
- products_count — 100;
- pharmacies_count — 10;
- products_with_images — близько 100.


4. Налаштування пароля MySQL

Найчастіша причина помилки запуску — неправильний пароль користувача root у файлі appsettings.json.

Якщо при запуску з'являється помилка:

   Access denied for user 'root'@'localhost' (using password: YES)

це означає, що у файлі appsettings.json вказано пароль від MySQL іншого комп'ютера.

Потрібно відкрити файл:

   AIPharma_publish\appsettings.json

і знайти блок підключення до бази даних:

"ConnectionStrings": {
  "DefaultConnection": "server=localhost;port=3306;database=aipharma_db;user=root;password=YOUR_PASSWORD;"
}

Замість YOUR_PASSWORD потрібно вписати пароль від MySQL на поточному комп'ютері.

Наприклад, якщо пароль MySQL дорівнює 123456, рядок має виглядати так:

"ConnectionStrings": {
  "DefaultConnection": "server=localhost;port=3306;database=aipharma_db;user=root;password=123456;"
}

Якщо користувач root у MySQL створений без пароля, тоді потрібно залишити password порожнім:

"ConnectionStrings": {
  "DefaultConnection": "server=localhost;port=3306;database=aipharma_db;user=root;password=;"
}

Після зміни appsettings.json файл потрібно зберегти.


5. Налаштування AI-модуля

Для демонстраційного запуску без зовнішнього API-ключа бажано залишити тестовий режим:

"LLM": {
  "UseMock": true,
  "Provider": "OpenAI",
  "ApiKey": "",
  "Model": "gpt-5-nano",
  "MaxOutputTokens": 800
}

У такому режимі сайт працює без реального ключа OpenAI API. Логіка AI-модуля, історія діалогів, кешування відповідей та робота з базою знань залишаються доступними для демонстрації.

Якщо потрібно перевірити реальне звернення до LLM API, необхідно:

1. Вказати дійсний API-ключ у полі ApiKey.
2. Змінити UseMock на false.
3. Переконатися, що на акаунті OpenAI API є доступний баланс.

Для передачі проєкту на перевірку не рекомендується залишати особистий API-ключ у файлі appsettings.json.


6. Запуск сайту

Після імпорту бази та налаштування пароля MySQL потрібно відкрити папку:

   AIPharma_publish

і запустити файл:

   run.bat

Після запуску сайт має відкритися у браузері за адресою:

   http://localhost:5000

Якщо браузер відкрив сторінку з помилкою ERR_CONNECTION_REFUSED, це означає, що веб-застосунок не запустився або одразу завершив роботу з помилкою.

У такому випадку потрібно запустити проєкт через консоль:

1. Відкрити папку AIPharma_publish.
2. У адресному рядку провідника ввести:

   cmd

3. У відкритій консолі виконати:

   AIPharma.exe

4. Переглянути текст помилки у консолі.


7. Тестові облікові записи

Після коректного імпорту бази даних можна використовувати тестові акаунти:

Адміністратор:
admin@aipharma.com
admin123

Менеджер:
manager1@aipharma.com
manager123

Користувач:
user@aipharma.com
user123

Якщо ці акаунти не працюють, потрібно перевірити, чи була імпортована саме фінальна база даних з даними, а не лише структура таблиць.


8. Типові помилки та їх вирішення

1. Помилка:

   Access denied for user 'root'@'localhost'

Причина:

   Неправильний пароль MySQL у файлі appsettings.json.

Рішення:

   Вписати правильний пароль користувача root у ConnectionStrings.


2. Помилка:

   Unknown database 'aipharma_db'

Причина:

   База даних не імпортована або має іншу назву.

Рішення:

   Імпортувати файл aipharma_db_final.sql через MySQL Workbench.


3. Сайт відкрився, але немає товарів, акаунтів або зображень

Причина:

   Імпортована порожня база, стара база або лише структура без даних.

Рішення:

   Видалити стару базу та імпортувати фінальний дамп повторно.

SQL-команда для видалення старої бази:

DROP DATABASE IF EXISTS aipharma_db;

Після цього потрібно повторити імпорт фінального SQL-файлу.


4. Помилка ERR_CONNECTION_REFUSED у браузері

Причина:

   AIPharma.exe не запущений або завершив роботу через помилку.

Рішення:

   Запустити AIPharma.exe через cmd і переглянути текст помилки.


9. Короткий порядок запуску

1. Розпакувати архів.
2. Встановити або запустити MySQL Server.
3. Імпортувати aipharma_db_final.sql у MySQL Workbench.
4. Відкрити appsettings.json.
5. Змінити password у ConnectionStrings на пароль MySQL поточного комп'ютера.
6. Перевірити, що database=aipharma_db.
7. Запустити run.bat.
8. Відкрити http://localhost:5000.
9. Увійти через тестовий акаунт.


10. Примітка

Проєкт AIPharma є веб-системою аптеки з інтелектуальним модулем обробки запитів. Основна логіка AI-помічника включає використання системних промптів, бази готових відповідей, кешування частих питань, збереження історії діалогів та можливість інтеграції із зовнішнім API великої мовної моделі.

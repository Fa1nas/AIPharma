namespace AIPharma.Services
{
    public class MockLlmService : ILlmService
    {
        public Task<string> GenerateAnswerAsync(string systemPrompt, List<string> contextMessages, string userQuestion)
        {
            var question = userQuestion.ToLower();

            string answer;

            if (question.Contains("ціна") || question.Contains("цена") || question.Contains("стоимость"))
            {
                answer = "Я можу допомогти знайти товар у каталозі AIPharma та перевірити його ціну. Для точного перегляду ціни відкрийте сторінку товару або скористайтеся пошуком у каталозі.";
            }
            else if (question.Contains("аптек") || question.Contains("адрес") || question.Contains("карта"))
            {
                answer = "Список аптек AIPharma доступний на сторінці «Аптеки». Там можна переглянути адреси, телефони, графік роботи та розташування аптек на карті.";
            }
            else if (question.Contains("замов") || question.Contains("заказ"))
            {
                answer = "Щоб оформити замовлення, відкрийте сторінку товару, оберіть аптеку, в якій він є в наявності, вкажіть кількість і натисніть «Оформити». Оплата здійснюється при отриманні.";
            }
            else if (question.Contains("ліку") || question.Contains("доз") || question.Contains("болит") || question.Contains("симптом"))
            {
                answer = "Я не встановлюю діагноз, не призначаю лікування і не змінюю рекомендації лікаря. З медичних питань, дозування або симптомів краще звернутися до лікаря або фармацевта.";
            }
            else
            {
                answer = "Я AI-помічник AIPharma. Можу допомогти з пошуком товарів, наявністю в аптеках, знижками, замовленнями, обраними товарами, порівнянням і роботою сайту.";
            }

            return Task.FromResult(answer);
        }
    }
}
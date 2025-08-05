document.addEventListener('DOMContentLoaded', function () {
    const chatForm = document.getElementById('chatForm');
    const userInput = document.getElementById('userInput');
    const chatbox = document.getElementById('chatbox');

    chatForm.addEventListener('submit', async function (e) {
        e.preventDefault();
        const message = userInput.value.trim();
        if (!message) return;
        
        // Kullanıcı mesajını ekle
        appendMessage('Siz', message, 'user');
        userInput.value = '';
        chatbox.scrollTop = chatbox.scrollHeight;
        
        try {
            const response = await fetch('/Chat/SendMessage', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message })
            });
            
            if (response.ok) {
                const data = await response.json();
                appendMessage('Chatbot', data.reply, 'bot');
                chatbox.scrollTop = chatbox.scrollHeight;
                
                // Tüzel abonelik tespit edildiğinde otomatik yönlendirme
                if (data.redirectToCanliDestek) {
                    // Delay parametresi varsa onu kullan, yoksa 2 saniye
                    const delay = data.delay || 2000;
                    setTimeout(() => {
                        window.location.href = '/Chat/Start?scenario=CanliDestek';
                    }, delay);
                }
            } else {
                appendMessage('Chatbot', 'Bir hata oluştu. Lütfen tekrar deneyiniz.', 'bot');
            }
        } catch (error) {
            appendMessage('Chatbot', 'Bağlantı hatası oluştu. Lütfen tekrar deneyiniz.', 'bot');
        }
    });

    function appendMessage(sender, text, type) {
        const msgDiv = document.createElement('div');
        msgDiv.className = 'd-flex ' + (type === 'user' ? 'justify-content-end' : 'justify-content-start') + ' mb-2';
        
        const bubbleDiv = document.createElement('div');
        bubbleDiv.className = 'chat-bubble ' + (type === 'user' ? 'user' : 'bot');
        
        // Metni doğrudan bubble div'inin içine yerleştir
        bubbleDiv.innerHTML = text.replace(/\n/g, '<br>');
        msgDiv.appendChild(bubbleDiv);
        
        chatbox.appendChild(msgDiv);
    }
}); 
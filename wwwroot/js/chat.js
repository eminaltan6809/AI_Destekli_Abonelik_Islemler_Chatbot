document.addEventListener('DOMContentLoaded', function () {
    const chatForm = document.getElementById('chatForm');
    const userInput = document.getElementById('userInput');
    const chatbox = document.getElementById('chatbox');

    chatForm.addEventListener('submit', async function (e) {
        e.preventDefault();
        const message = userInput.value.trim();
        if (!message) return;
        appendMessage('Siz', message, 'user');
        userInput.value = '';
        chatbox.scrollTop = chatbox.scrollHeight;
        const response = await fetch('/Chat/SendMessage', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ message })
        });
        if (response.ok) {
            const data = await response.json();
            appendMessage('Chatbot', data.reply, 'bot');
            chatbox.scrollTop = chatbox.scrollHeight;
        } else {
            appendMessage('Chatbot', 'Bir hata oluştu.', 'bot');
        }
    });

    function appendMessage(sender, text, type) {
        const msgDiv = document.createElement('div');
        msgDiv.className = 'mb-2 ' + (type === 'user' ? 'text-end' : 'text-start');
        msgDiv.innerHTML = `<span class="badge bg-${type === 'user' ? 'primary' : 'secondary'}">${sender}:</span> <span>${text}</span>`;
        chatbox.appendChild(msgDiv);
    }
}); 
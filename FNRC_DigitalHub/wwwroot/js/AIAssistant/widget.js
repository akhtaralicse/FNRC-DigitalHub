/* AIAssistant Widget Logic */

(function () {
    const ASSETS = {
        logo: 'https://cdn-icons-png.flaticon.com/512/2040/2040946.png', // Modern AI Icon
        botIcon: 'https://cdn-icons-png.flaticon.com/512/4712/4712035.png'
    };

    let isMinimized = true;
    let currentLang = 'EN'; // EN or AR
    const chatHistory = [];
    const sessionId = 'session_' + Math.random().toString(36).substr(2, 9);

    const INITIAL_SUGGESTIONS = {
        EN: [
            "What are the services for individuals?",
            "How can I contact FNRC headquarters?",
            "What is the vision and mission of FNRC?"
        ],
        AR: [
            "ما هي الخدمات المقدمة للأفراد؟",
            "كيف يمكنني التواصل مع المقر الرئيسي للمؤسسة؟",
            "ما هي رؤية ورسالة المؤسسة؟"
        ]
    };

    const UI = {
        init: () => {
            const widget = document.createElement('div');
            widget.id = 'ai-assistant-widget';
            widget.innerHTML = `
                <div id="ai-assistant-window">
                    <div id="ai-assistant-header">
                        <div style="display:flex; align-items:center; gap:10px">
                            <img src="${ASSETS.botIcon}" width="24" height="24" />
                            <span id="ai-title">FNRC AI Assistant</span>
                        </div>
                        <div style="display:flex; gap:8px">
                            <button id="ai-lang-switch" class="lang-switch">AR</button>
                            <button id="ai-close" style="background:none; border:none; color:white; font-size:18px; cursor:pointer;">&times;</button>
                        </div>
                    </div>
                    <div id="ai-assistant-chat-container">
                        <div class="ai-message bot" id="ai-welcome">Welcome to FNRC! How can I assist you today?</div>
                        <div id="initial-suggestions" class="suggestion-container"></div>
                    </div>
                    <div id="ai-assistant-input-area">
                        <input type="text" id="ai-assistant-input" placeholder="Ask about FNRC..." />
                        <button id="ai-assistant-send">
                           <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 2L11 13M22 2l-7 20-4-9-9-4 20-7z"></path></svg>
                        </button>
                    </div>
                </div>
                <button id="ai-assistant-toggle">
                     <img src="${ASSETS.logo}" width="35" height="35" style="filter: invert(1);" />
                </button>
            `;
            document.body.appendChild(widget);

            // Event Listeners
            document.getElementById('ai-assistant-toggle').addEventListener('click', UI.toggle);
            document.getElementById('ai-close').addEventListener('click', UI.toggle);
            document.getElementById('ai-lang-switch').addEventListener('click', UI.switchLang);
            document.getElementById('ai-assistant-send').addEventListener('click', UI.sendMessage);
            document.getElementById('ai-assistant-input').addEventListener('keypress', (e) => {
                if (e.key === 'Enter') UI.sendMessage();
            });

            UI.updateTexts();
            UI.showInitialSuggestions();
        },

        toggle: () => {
            const window = document.getElementById('ai-assistant-window');
            isMinimized = !isMinimized;
            window.style.display = isMinimized ? 'none' : 'flex';
        },

        switchLang: () => {
            currentLang = currentLang === 'EN' ? 'AR' : 'EN';
            document.getElementById('ai-lang-switch').innerText = currentLang === 'EN' ? 'AR' : 'EN';
            UI.updateTexts();
            UI.showInitialSuggestions();

            const window = document.getElementById('ai-assistant-window');
            const input = document.getElementById('ai-assistant-input');
            if (currentLang === 'AR') {
                window.style.direction = 'rtl';
                input.style.direction = 'rtl';
            } else {
                window.style.direction = 'ltr';
                input.style.direction = 'ltr';
            }
        },

        updateTexts: () => {
            const texts = {
                EN: { title: 'FNRC AI Assistant', welcome: 'Welcome to FNRC! How can I assist you today?', placeholder: 'Ask about FNRC...' },
                AR: { title: 'مساعد مؤسسة الفجيرة', welcome: 'مرحباً بكم في مؤسسة الفجيرة للموارد الطبيعية! كيف يمكنني مساعدتكم اليوم؟', placeholder: 'اسأل عن خدمات المؤسسة...' }
            };
            document.getElementById('ai-title').innerText = texts[currentLang].title;
            document.getElementById('ai-assistant-input').placeholder = texts[currentLang].placeholder;
            if (chatHistory.length === 0) {
                document.getElementById('ai-welcome').innerText = texts[currentLang].welcome;
            }
        },

        showInitialSuggestions: () => {
            const container = document.getElementById('initial-suggestions');
            if (!container) return;

            container.innerHTML = '';
            INITIAL_SUGGESTIONS[currentLang].forEach(text => {
                const chip = document.createElement('div');
                chip.className = 'suggestion-chip';
                chip.innerText = text;
                chip.onclick = () => UI.handleSuggestion(text);
                container.appendChild(chip);
            });
        },

        handleSuggestion: (text) => {
            const input = document.getElementById('ai-assistant-input');
            input.value = text;
            UI.sendMessage();
            // Hide initial suggestions after first action
            const initial = document.getElementById('initial-suggestions');
            if (initial) initial.style.display = 'none';
        },

        sendMessage: async (customText = null) => {
            const input = document.getElementById('ai-assistant-input');
            const container = document.getElementById('ai-assistant-chat-container');
            const message = customText || input.value.trim();
            if (!message) return;

            // Clear suggestions if any
            const oldSuggestions = container.querySelectorAll('.suggestion-container');
            oldSuggestions.forEach(s => s.style.display = 'none');

            // Add user message to UI
            UI.appendMessage(message, 'user');
            input.value = '';

            // Loading state
            const loading = document.createElement('div');
            loading.className = 'typing-indicator';
            loading.innerText = currentLang === 'EN' ? 'FNRC AI is thinking...' : 'المساعد يفكر...';
            container.appendChild(loading);
            container.scrollTop = container.scrollHeight;

            try {
                const response = await fetch('/api/AIAssistant/chat', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ query: message, lang: currentLang, sessionId: sessionId })
                });

                const result = await response.json();
                if (container.contains(loading)) container.removeChild(loading);

                UI.appendMessage(result.ResponseText, 'bot', result.Sources, result.RecommendedQuestions);
            } catch (err) {
                if (container.contains(loading)) container.removeChild(loading);
                UI.appendMessage(currentLang === 'EN' ? 'Something went wrong.' : 'حدث خطأ ما.', 'bot');
            }
        },

        appendMessage: (text, sender, sources = [], recommendations = []) => {
            const container = document.getElementById('ai-assistant-chat-container');
            const div = document.createElement('div');
            div.className = `ai-message ${sender}`;

            let html = text;
            if (sources && sources.length > 0) {
                html += '<br/><small style="opacity:0.6; font-size:10px; display:block; margin-top:5px;">Sources: ' + sources.join(', ') + '</small>';
            }

            div.innerHTML = html;
            container.appendChild(div);

            // Add recommendations if bot
            if (sender === 'bot' && recommendations && recommendations.length > 0) {
                const recContainer = document.createElement('div');
                recContainer.className = 'suggestion-container';
                recommendations.forEach(q => {
                    const chip = document.createElement('div');
                    chip.className = 'suggestion-chip';
                    chip.innerText = q;
                    chip.onclick = () => UI.handleSuggestion(q);
                    recContainer.appendChild(chip);
                });
                container.appendChild(recContainer);
            }

            container.scrollTop = container.scrollHeight;
            chatHistory.push({ text, sender });

            // Hide initial suggestions if they are still visible
            const initial = document.getElementById('initial-suggestions');
            if (initial) initial.style.display = 'none';
        }
    };

    // Initial load
    if (document.readyState === 'complete') {
        UI.init();
    } else {
        window.addEventListener('load', UI.init);
    }
})();

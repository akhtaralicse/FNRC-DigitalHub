/* AIAssistant Widget Logic */

(function () {
    const ASSETS = {
        logo: 'https://img.icons8.com/fluency/96/artificial-intelligence.png', // Modern AI Icon
        botIcon: 'https://img.icons8.com/fluency/96/bot.png'
    };

    let isMinimized = true;
    let currentLang = 'EN'; // EN or AR
    const chatHistory = [];
    const sessionId = 'session_' + Math.random().toString(36).substr(2, 9);

    const UI = {

        init: () => {
            if (window.location.pathname.toLowerCase().includes('aiassistant')) return;

            const widget = document.createElement('div');

            widget.id = 'ai-assistant-widget';
            widget.innerHTML = `
                <div id="ai-assistant-window">
                    <div id="ai-assistant-header">
                        <div style="display:flex; align-items:center; gap:10px">
                            <img src="${ASSETS.botIcon}" width="24" height="24" />
                            <span id="ai-title">FNRC AI Agent</span>
                        </div>
                        <div style="display:flex; gap:10px; align-items:center;">
                            <button id="ai-lang-switch" style="background:rgba(255,255,255,0.2); border:1px solid rgba(255,255,255,0.5); color:white; border-radius:4px; padding:2px 6px; cursor:pointer; font-size:10px; font-weight:bold;" title="Switch Language">AR</button>
                            <button id="ai-expand" onclick="window.open('/AIAssistantChat', '_blank');" style="background:none; border:none; color:white; cursor:pointer;" title="Open Full Page Chat">
                                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 3h6v6M9 21H3v-6M21 3l-7 7M3 21l7-7"/></svg>
                            </button>
                            <button id="ai-close" style="background:none; border:none; color:white; cursor:pointer;" title="Close">
                                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                            </button>
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
                <button id="ai-assistant-toggle" style="position: relative;">
                    <span class="ai-badge">AI</span>
                    <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path>
                    </svg>
                </button>
            `;
            document.body.appendChild(widget);

            // Event Listeners - Wrapped to prevent passing event objects as text
            document.getElementById('ai-assistant-toggle').addEventListener('click', () => UI.toggle());
            document.getElementById('ai-close').addEventListener('click', () => UI.toggle());
            document.getElementById('ai-lang-switch').addEventListener('click', () => UI.switchLang());
            document.getElementById('ai-assistant-send').addEventListener('click', () => UI.sendMessage());
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
                EN: { title: 'FNRC AI Agent', welcome: 'Welcome to FNRC! How can I assist you today?', placeholder: 'Ask about FNRC...' },
                AR: { title: 'وكيل مؤسسة الفجيرة', welcome: 'مرحباً بكم في مؤسسة الفجيرة للموارد الطبيعية! كيف يمكنني مساعدتكم اليوم؟', placeholder: 'اسأل عن خدمات المؤسسة...' }
            };
            document.getElementById('ai-title').innerText = texts[currentLang].title;
            document.getElementById('ai-assistant-input').placeholder = texts[currentLang].placeholder;
            if (chatHistory.length === 0) {
                document.getElementById('ai-welcome').innerText = texts[currentLang].welcome;
            }
        },

        showInitialSuggestions: async () => {
            const container = document.getElementById('initial-suggestions');
            if (!container) return;

            container.innerHTML = '<div style="font-size:10px; opacity:0.5; margin-left:10px; padding:10px;">Loading suggestions...</div>';

            try {
                const response = await fetch(`/api/AIAssistant/suggestions?lang=${currentLang}`);
                const suggestions = await response.json();

                container.innerHTML = '';
                if (suggestions && suggestions.length > 0) {
                    suggestions.forEach(text => {
                        const chip = document.createElement('div');
                        chip.className = 'suggestion-chip';
                        chip.innerText = text;
                        chip.onclick = () => UI.handleSuggestion(text);
                        container.appendChild(chip);
                    });
                }
            } catch (err) {
                container.innerHTML = '';
                console.error("Failed to load suggestions", err);
            }
        },


        handleSuggestion: (text) => {
            const input = document.getElementById('ai-assistant-input');
            input.value = text;
            UI.sendMessage();
            // Hide initial suggestions after first action
            const initial = document.getElementById('initial-suggestions');
            if (initial) initial.style.display = 'none';
        },

        sendMessage: async (arg = null) => {
            const input = document.getElementById('ai-assistant-input');
            const container = document.getElementById('ai-assistant-chat-container');

            // 1. Extract message: if 'arg' is a string (from chip), use it. Otherwise use input field.
            let message = (typeof arg === 'string' ? arg : input.value) || "";
            message = message.toString().trim();

            if (!message) return;

            // 2. Clear input field
            input.value = '';

            const oldSuggestions = container.querySelectorAll('.suggestion-container');
            oldSuggestions.forEach(s => s.style.display = 'none');

            // Add user message to UI
            UI.appendMessage(message, 'user');
            input.value = '';

            // Loading state
            const loading = document.createElement('div');
            loading.className = 'typing-indicator';
            loading.innerText = currentLang === 'EN' ? 'FNRC AI Agent is thinking...' : 'وكيل مؤسسة الفجيرة يفكر...';
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

                UI.appendMessage(result.ResponseText, 'bot', result.Sources, result.RecommendedQuestions, result.LogId);
            } catch (err) {
                if (container.contains(loading)) container.removeChild(loading);
                UI.appendMessage(currentLang === 'EN' ? 'Something went wrong.' : 'حدث خطأ ما.', 'bot');
            }
        },

        submitFeedback: async (logId, isPositive, btn) => {
            try {
                const response = await fetch('/api/AIAssistant/submit-feedback', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ LogId: logId, IsPositive: isPositive })
                });
                if (response.ok) {
                    const parent = btn.parentElement;
                    const btns = parent.querySelectorAll('button');

                    // Disable all buttons in this feedback group
                    btns.forEach(b => {
                        b.disabled = true;
                        b.style.pointerEvents = 'none';
                        b.style.opacity = '0.3';
                    });

                    // Highlight the chosen one
                    btn.style.opacity = '1';
                    btn.style.color = isPositive ? '#28a745' : '#dc3545'; // Green or Red
                    btn.style.transform = 'scale(1.2)';
                }
            } catch (err) {
                console.error("Feedback failed", err);
            }
        },



        appendMessage: (text, sender, sources = [], recommendations = [], logId = null) => {
            const container = document.getElementById('ai-assistant-chat-container');
            const div = document.createElement('div');
            div.className = `ai-message ${sender}`;
            if (logId) div.setAttribute('data-log-id', logId);

            let html = text;
            if (sources && sources.length > 0) {
                html += '<br/><small style="opacity:0.6; font-size:10px; display:block; margin-top:5px;">Sources: ' + sources.join(', ') + '</small>';
            }

            // Add Feedback icons for bot messages
            if (sender === 'bot' && logId) {
                html += `
                <div class="ai-feedback-btns" style="margin-top:10px; border-top:1px solid rgba(0,0,0,0.05); padding-top:5px; display:flex; gap:10px;">
                    <button onclick="UI.submitFeedback(${logId}, true, this)" style="background:none; border:none; cursor:pointer; opacity:0.5; padding:0"><i class="fa fa-thumbs-up"></i></button>
                    <button onclick="UI.submitFeedback(${logId}, false, this)" style="background:none; border:none; cursor:pointer; opacity:0.5; padding:0"><i class="fa fa-thumbs-down"></i></button>
                </div>`;
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

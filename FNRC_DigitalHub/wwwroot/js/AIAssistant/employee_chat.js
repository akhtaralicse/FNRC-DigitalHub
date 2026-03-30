/* Employee Chat Logic */

(function () {
    const ASSETS = {
        chatIcon: 'https://cdn-icons-png.flaticon.com/512/3193/3193015.png'
    };

    let connection = null;
    let currentUser = null;
    let otherUser = null;
    let isMinimized = true;

    const UI = {
        init: async () => {
            console.log("Employee Chat: Initializing...");
            // 1. Get current user
            try {
                const meRes = await fetch('/api/EmployeeChat/me');
                if (!meRes.ok) {
                    console.warn("Employee Chat: User not authenticated. Widget will not be shown.");
                    return;
                }
                currentUser = await meRes.json();
                console.log("Employee Chat: Logged in as", currentUser.NameEn);
                if (!currentUser) return;
            } catch (err) {
                console.error("Employee Chat: Failed to fetch user session", err);
                return;
            }

            // 2. Load SignalR (CDN)
            const script = document.createElement('script');
            script.src = 'https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/6.0.1/signalr.min.js';
            script.onload = UI.setupSignalR;
            document.head.appendChild(script);

            // 3. Render Widget
            const widget = document.createElement('div');
            widget.id = 'emp-chat-widget';
            widget.innerHTML = `
                <div id="emp-chat-window">
                    <div id="emp-chat-header">
                        <span id="chat-header-title">Colleague Chat</span>
                        <div style="display:flex; gap:10px">
                             <button id="chat-back" style="display:none; color:white; background:none; border:none; cursor:pointer;">&larr; Back</button>
                             <button id="chat-close" style="color:white; background:none; border:none; cursor:pointer;">&times;</button>
                        </div>
                    </div>
                    <div id="emp-chat-list">
                         <div style="padding:10px; opacity:0.6">Loading coworkers...</div>
                    </div>
                    <div id="emp-chat-pane">
                         <div id="emp-chat-messages"></div>
                         <div id="emp-chat-input-area">
                             <input type="text" id="emp-chat-input" placeholder="Type a message..." />
                             <button id="emp-chat-send" style="background:#4A90E2; color:white; border:none; border-radius:4px; padding:5px 10px; cursor:pointer;">Send</button>
                         </div>
                    </div>
                </div>
                <button id="emp-chat-toggle" title="Chat with Colleagues">
                     <span style="font-size:24px; color:white;">💬</span>
                </button>
             `;
            document.body.appendChild(widget);

            document.getElementById('emp-chat-toggle').onclick = UI.toggle;
            document.getElementById('chat-close').onclick = UI.toggle;
            document.getElementById('chat-back').onclick = UI.showUserList;
            document.getElementById('emp-chat-send').onclick = UI.sendMessage;
            document.getElementById('emp-chat-input').onkeypress = (e) => {
                if (e.key === 'Enter') UI.sendMessage();
            };

            UI.loadUsers();
        },

        setupSignalR: () => {
            connection = new signalR.HubConnectionBuilder()
                .withUrl("/chatHub")
                .build();

            connection.on("UserStatusChange", (userId, isOnline) => {
                const row = document.querySelector(`[data-uid="${userId}"] .status-dot`);
                if (row) {
                    row.className = `status-dot ${isOnline ? 'online' : ''}`;
                }
            });

            connection.on("ReceiveMessage", (senderId, message, date) => {
                if (otherUser && otherUser.Id === senderId) {
                    UI.appendMessage(message, 'them', date);
                } else {
                    // Show notification badge or similar
                    console.log("New message from", senderId);
                }
            });

            connection.start().then(() => {
                connection.invoke("Join", currentUser.Id);
            });
        },

        toggle: () => {
            const window = document.getElementById('emp-chat-window');
            isMinimized = !isMinimized;
            window.style.display = isMinimized ? 'none' : 'flex';
        },

        showUserList: () => {
            document.getElementById('emp-chat-list').style.display = 'block';
            document.getElementById('emp-chat-pane').style.display = 'none';
            document.getElementById('chat-back').style.display = 'none';
            document.getElementById('chat-header-title').innerText = 'Colleague Chat';
            otherUser = null;
        },

        loadUsers: async () => {
            const list = document.getElementById('emp-chat-list');
            try {
                const res = await fetch('/api/EmployeeChat/users');
                const users = await res.json();
                list.innerHTML = users.map(u => `
                    <div class="chat-user-item" data-uid="${u.Id}" onclick="UI.startChat(${JSON.stringify(u).replace(/"/g, '&quot;')})">
                        <div class="status-dot ${u.IsOnline ? 'online' : ''}"></div>
                        <div>
                            <div style="font-size:14px; font-weight:600">${u.NameEn}</div>
                            <div style="font-size:12px; opacity:0.6">${u.DepartmentNameEn || '---'}</div>
                        </div>
                    </div>
                `).join('');
            } catch {
                list.innerHTML = '<div style="padding:10px; color:red">Error loading users.</div>';
            }
        },

        startChat: async (user) => {
            otherUser = user;
            document.getElementById('emp-chat-list').style.display = 'none';
            document.getElementById('emp-chat-pane').style.display = 'flex';
            document.getElementById('chat-back').style.display = 'block';
            document.getElementById('chat-header-title').innerText = user.NameEn;

            const msgContainer = document.getElementById('emp-chat-messages');
            msgContainer.innerHTML = '<div style="text-align:center; padding:10px; opacity:0.5">Loading history...</div>';

            try {
                const res = await fetch(`/api/EmployeeChat/history/${user.Id}`);
                const history = await res.json();
                msgContainer.innerHTML = '';
                history.forEach(m => {
                    UI.appendMessage(m.Message, m.SenderId === currentUser.Id ? 'me' : 'them', m.SentDate);
                });
            } catch {
                msgContainer.innerHTML = 'Error loading history.';
            }
        },

        sendMessage: () => {
            const input = document.getElementById('emp-chat-input');
            const message = input.value.trim();
            if (!message || !otherUser) return;

            connection.invoke("SendMessage", currentUser.Id, otherUser.Id, message);
            UI.appendMessage(message, 'me');
            input.value = '';
        },

        appendMessage: (text, sender, date) => {
            const container = document.getElementById('emp-chat-messages');
            const div = document.createElement('div');
            div.className = `emp-msg ${sender}`;
            div.innerText = text;
            container.appendChild(div);
            container.scrollTop = container.scrollHeight;
        }
    };

    if (document.readyState === 'complete') {
        UI.init();
    } else {
        window.addEventListener('load', UI.init);
    }
})();

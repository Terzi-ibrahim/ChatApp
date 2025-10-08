"use strict";

// Mevcut kullanıcıyı alıyoruz
const currentUserId = parseInt(document.getElementById("currentUserId").value);
let currentChatUserId = null;

// Chat mesajları container
const chatMessages = document.getElementById("chatMessages");

// SignalR Chat Hub bağlantısı
const chatConnection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .withAutomaticReconnect()
    .build();

// Bağlantıyı başlat
chatConnection.start().catch(err => console.error("SignalR connect error:", err));

// Üyeye tıklayınca mesajları yükle
document.getElementById("memberList").addEventListener("click", function (e) {
    const item = e.target.closest(".member-item");
    if (!item) return;
    e.preventDefault();

    const li = item.closest("li");
    currentChatUserId = li.dataset.userid;

    // Seçili kullanıcıyı göster
    document.querySelectorAll("#memberList li").forEach(el => el.classList.remove("bg-body-tertiary"));
    li.classList.add("bg-body-tertiary");

    // Mesajları çek
    fetch(`/Chat/GetMessages?userId=${currentChatUserId}`)
        .then(res => res.json())
        .then(messages => {
            chatMessages.innerHTML = "";
            messages.forEach(msg => appendMessage(msg));
            scrollToBottom();

            // Mesajları okundu yap
            fetch(`/Chat/MarkAsRead?userId=${currentChatUserId}`, { method: "POST" });
        })
        .catch(err => console.error("GetMessages error:", err));
});

// Mesaj gönderme (text + resim)
document.getElementById("sendButton").addEventListener("click", function () {
    const messageInput = document.getElementById("chatInput");
    const fileInput = document.getElementById("chatImage");

    const message = messageInput.value.trim();
    const file = fileInput.files[0];

    if (!message && !file) return;
    if (!currentChatUserId) return;

    const formData = new FormData();
    formData.append("recipientId", currentChatUserId);
    if (message) formData.append("message", message);
    if (file) formData.append("image", file);

    fetch("/Chat/SendMessage", { method: "POST", body: formData })
        .then(() => {
            // inputları temizle
            messageInput.value = "";
            fileInput.value = "";
        })
        .catch(err => console.error("SendMessage error:", err));
});

// Sohbeti temizle
document.getElementById("clearChatButton").addEventListener("click", function () {
    if (!currentChatUserId) return;

    fetch(`/Chat/ClearMessages?userId=${currentChatUserId}`, { method: "POST" })
        .then(() => {
            chatMessages.innerHTML = "";
        })
        .catch(err => console.error("ClearMessages error:", err));
});

// Gelen mesajları dinle
chatConnection.on("ReceiveMessage", function (msg) {
    // Eğer mesaj başka kullanıcıdan geldiyse badge göster
    if (msg.senderId != currentUserId && msg.senderId != currentChatUserId) {
        const li = document.querySelector(`#memberList li[data-userid='${msg.senderId}']`);
        if (li) {
            li.classList.add("bg-body-tertiary");
            const badge = li.querySelector(".badge");
            if (badge) badge.innerText = parseInt(badge.innerText || "0") + 1;
            else {
                const span = document.createElement("span");
                span.classList.add("badge", "bg-danger", "float-end");
                span.innerText = "1";
                li.querySelector(".pt-1").after(span);
            }
        }
    }

    // Eğer mesaj şu anda açık olan sohbetten geldiyse ekle
    if (msg.senderId == currentChatUserId || msg.senderId == currentUserId) {
        appendMessage(msg);
        scrollToBottom();
    }
});

// Mesajları chatMessages içine ekleyen yardımcı fonksiyon
function appendMessage(msg) {
    const li = document.createElement("li");
    li.classList.add("d-flex", "mb-4");
    li.classList.add(msg.senderId == currentUserId ? "sent" : "received");

    li.innerHTML = msg.senderId == currentUserId
        ? `<div class="card w-100 ms-3">
                <div class="card-body">
                    ${msg.message ? `<p>${msg.message}</p>` : ""}
                    ${msg.imageUrl ? `<img src="${msg.imageUrl}" class="img-fluid mt-1"/>` : ""}
                    <small class="text-muted">${new Date(msg.createAt).toLocaleTimeString()}</small>
                </div>
           </div>`
        : `<img src="/img/user.jpg" class="rounded-circle me-3" width="60">
           <div class="card w-100">
               <div class="card-body">
                   ${msg.message ? `<p>${msg.message}</p>` : ""}
                   ${msg.imageUrl ? `<img src="${msg.imageUrl}" class="img-fluid mt-1"/>` : ""}
                   <small class="text-muted">${new Date(msg.createAt).toLocaleTimeString()}</small>
               </div>
           </div>`;

    chatMessages.appendChild(li);
}

// Scrollu chat alanı içinde en alta kaydır
function scrollToBottom() {
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

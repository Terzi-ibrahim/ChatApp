"use strict";

// SignalR bağlantısını başlat
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/activeUsersHub")
    .build();

// Hub'dan gelen aktif kullanıcı sayısını dinle
connection.on("UpdateActiveUserCount", function (count) {
    const counter = document.getElementById("activeUserCount");
    if (counter) {
        counter.textContent = count;
    }
});

// Bağlantıyı başlat
connection.start()
    .then(() => console.log("SignalR connected ✅"))
    .catch(err => console.error("SignalR connection error ❌", err));

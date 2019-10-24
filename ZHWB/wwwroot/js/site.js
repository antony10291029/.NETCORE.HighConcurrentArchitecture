// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function () {
    getPubkey();
    register("guest", "guest");
    remove("testid", "notoken");
    testPolicy("notoken");
    function initsignalR(token){
        var connection = new signalR.HubConnectionBuilder().withUrl("/notifyHub?access_token="+token).build();
        document.getElementById("sendButton").disabled = true;
        connection.on("publicMessageReceived", function (message) {
            var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
            var li = document.createElement("li");
            li.textContent = msg;
            document.getElementById("messagesList").appendChild(li);
        });
        connection.on("onConnected", function (uid,cid) {
            $("#signalRTest").append("<br/> connected ok");
            document.getElementById("userInput").innerText=uid;
            document.getElementById("connInput").innerText=cid;
            $("#signalRTest").addClass("text-success");
        });
        connection.start().then(function () {
            document.getElementById("sendButton").disabled = false;
        }).catch(function (err) {
            return console.error(err.toString());
        });
    
        document.getElementById("sendButton").addEventListener("click", function (event) {
            var message = document.getElementById("messageInput").value;
            connection.invoke("sendPublicMessage", message).catch(function (err) {
                return console.error(err.toString());
            });
            event.preventDefault();
        });
    }
    function getPubkey() {
        $.ajax({
            url: "api/auth/GetKey",
            success: function (data) {
                Login("admin", "admin", data);
                $("#getTest").addClass("text-success");
                $("#getTest").append("<br/>ok");
            },
            error: function (err) {
                $("#getTest").addClass("text-danger");
                $("#getTest").append("<br/>" + err.statusText);
            }
        });
    }

    function register(uname, pwd) {
        $.ajax({
            url: "api/auth/Register",
            type: "post",
            data: { username: uname, password: pwd },
            success: function (data) {
                $("#postTest").addClass("text-success");
                $("#postTest").append("<br/>register " + uname + ":" + data);
            },
            error: function (err) {
                $("#postTest").addClass("text-danger");
                $("#postTest").append(err.statusText);
            }
        });
    }
    function remove(uid, token_data) {
        $.ajax({
            url: "api/auth/removeUser",
            headers: { "Authorization": "Bearer " + token_data },
            type: "post",
            data: { id: uid },
            success: function (data) {
                $("#apiAuthTestOK").addClass("text-success");
                $("#apiAuthTestOK").append("<br/>ok");
            },
            error: function (err) {
                $("#apiAuthTestFAIL").addClass("text-danger");
                $("#apiAuthTestFAIL").append("<br/>" + err.statusText);
            }
        });
    }
    function testPolicy(token_data) {
        $.ajax({
            url: "api/auth/testPolicy",
            headers: { "Authorization": "Bearer " + token_data },
            type: "post",
            success: function (data) {
                $("#testPolicyOK").addClass("text-success");
                $("#testPolicyOK").append("<br/>ok");

            },
            error: function (err) {
                $("#testPolicyFAIL").addClass("text-danger");
                $("#testPolicyFAIL").append("<br/>" + err.statusText);
            }
        });
    }
    function Login(uname, pwd, publickey) {
        var obj = { username: uname, password: pwd };
        var encrypt = new JSEncrypt();
        encrypt.setPublicKey(publickey);
        var test = encrypt.encrypt(JSON.stringify(obj).trim());
        $.ajax({
            url: "api/auth/Login",
            type: "post",
            data: { logininfo: test },
            success: function (token) {
                $("#loginTest").addClass("text-success");
                $("#loginTest").append("<br/>token:" + token);
                remove("testid", token);
                testPolicy(token);
                initsignalR(token);
                $("#pageTest").append("<a target='_blank' href='home/about?access_token=" + token + "'>authPageOK</a><br/><a target='_blank' href='home/about?access_token=nodata'>authPageFAIL</a>");
            },
            error: function (err) {
                $("#loginTest").addClass("text-danger");
                $("#loginTest").append("<br/>" + err.statusText);

            }
        });
    }
});
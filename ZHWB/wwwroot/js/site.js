// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function(){
    getPubkey();
    register("admin","admin");
    register("guest","guest");
    Login("admin","admin",$("#pubkey").text());
    function getPubkey(){
        $.ajax({
            url:"api/auth/GetKey",
            success:function(data){
                $("#getTest").append("<br/>ok");
    
            },
            error:function(err){
                $("#getTest").append(err.statusText);
            }
        });
    }
    
    function register(uname,pwd){
        $.ajax({
            url:"api/auth/Register",
            type:"post",
            data:{username:uname,password:pwd},
            success:function(data){
                $("#postTest").append("<br/>register "+uname+":"+data);
    
            },
            error:function(err){
                $("#postTest").append(err.statusText);
            }
        });
    }

    function Login(uname,pwd,publickey){
        var obj={username:uname,password:pwd};
        var encrypt = new JSEncrypt();
        encrypt.setPublicKey(publickey);
        var test = encrypt.encrypt(JSON.stringify(obj).trim());
        $.ajax({
            url:"api/auth/Login",
            type:"post",
            data:{logininfo:test},
            success:function(data){
                $("#loginTest").append("<br/>token:"+data);
    
            },
            error:function(err){
                $("#loginTest").append(err.statusText);
            }
        });
    }
});
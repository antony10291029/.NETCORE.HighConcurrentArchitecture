// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function(){
    getPubkey();
    register("guest","guest");
    
    remove("testid","notoken");
    testPolicy("notoken");
    function getPubkey(){
        $.ajax({
            url:"api/auth/GetKey",
            success:function(data){
                Login("admin","admin",data);
                $("#getTest").addClass("text-success");
                $("#getTest").append("<br/>ok");
            },
            error:function(err){
                $("#getTest").addClass("text-danger");
                $("#getTest").append("<br/>"+err.statusText);
            }
        });
    }
    
    function register(uname,pwd){
        $.ajax({
            url:"api/auth/Register",
            type:"post",
            data:{username:uname,password:pwd},
            success:function(data){
                $("#postTest").addClass("text-success");
                $("#postTest").append("<br/>register "+uname+":"+data);
            },
            error:function(err){
                $("#postTest").addClass("text-danger");
                $("#postTest").append(err.statusText);
            }
        });
    }
    function remove(uid,token_data){
        $.ajax({
            url:"api/auth/removeUser",
            headers:{"Authorization": "Bearer "+token_data},
            type:"post",
            data:{id:uid},
            success:function(data){
                $("#apiAuthTestOK").addClass("text-success");
                $("#apiAuthTestOK").append("<br/>ok");
            },
            error:function(err){
                $("#apiAuthTestFAIL").addClass("text-danger");
                $("#apiAuthTestFAIL").append("<br/>"+err.statusText);
            }
        });
    }
    function testPolicy(token_data){
        $.ajax({
            url:"api/auth/testPolicy",
            headers:{"Authorization": "Bearer "+token_data},
            type:"post",
            success:function(data){
                $("#testPolicyOK").addClass("text-success");
                $("#testPolicyOK").append("<br/>ok");
    
            },
            error:function(err){
                $("#testPolicyFAIL").addClass("text-danger");
                $("#testPolicyFAIL").append("<br/>"+err.statusText);
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
                $("#loginTest").addClass("text-success");
                $("#loginTest").append("<br/>token:"+data);
                remove("testid",data);
                testPolicy(data);
            },
            error:function(err){
                $("#loginTest").addClass("text-danger");
                $("#loginTest").append("<br/>"+err.statusText);
            }
        });
    }
});
<?php 
    include("functions.php");

    if (isset($_POST["user-name"]) 
        and !empty($_POST["user-name"])) {
  
        insert_new_user($_POST["user-name"]);
        header("Location: index.php");
            
    } else {
        echo "<script type='text/javascript'>alert('Invalid data');</script>";
        echo "<script type='text/javascript'>location.href='index.php';</script>";
    }
?>
<?php 
    include("functions.php");

    if (isset($_POST["name"], $_POST["genres"]) 
        and !empty($_POST["name"]) && !empty($_POST["genres"])) {
  
        insert_new_movie($_POST["name"], $_POST["genres"]);
        header("Location: index.php");
            
    } else {
        echo "<script type='text/javascript'>alert('Invalid data');</script>";
        echo "<script type='text/javascript'>location.href='index.php';</script>";
    }
?>
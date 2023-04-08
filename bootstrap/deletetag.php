<?php 
    include("functions.php");
    $userId = $_GET['userId'];
    $movieId = $_GET['movieId'];
    $tag = $_GET['tag'];

    if ($userId and $movieId and $tag) {
        delete_tag($userId, $movieId, $tag);

        header("Location: index.php");
    } else {
        echo "<script type='text/javascript'>alert('Tag doesn't exist!');</script>";
        echo "<script type='text/javascript'>location.href='index.php';</script>";
    }
?>

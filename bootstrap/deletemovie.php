<?php 
    include("functions.php");
    $id = $_GET['id'];

    if ($id) {
        delete_movie($id);

        header("Location: index.php");
    } else {
        echo "<script type='text/javascript'>alert('Movie doesn't exist');</script>";
        echo "<script type='text/javascript'>location.href='index.php';</script>";
    }
?>

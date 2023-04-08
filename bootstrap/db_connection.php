<?php 
    $servername = "localhost";
    $username = "root";
    $password = "";
    $dbname = "movie_lens";

    $conn = mysqli_connect($servername, $username, $password, $dbname);

    if (!$conn) {
        die("Connection error" . mysqli_connect_error());
    }
?>
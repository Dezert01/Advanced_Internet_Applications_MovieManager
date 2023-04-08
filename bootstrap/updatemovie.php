<?php 
    require_once ("functions.php");
    include ("header.php");
    session_start();
    if (isset($_GET['id'])) {
        $id = $_GET['id'];
        $_SESSION['id'] = $id;
    } else if (isset($_POST["update-name"], $_POST["update-genres"], $_SESSION['id'])
        and !empty($_POST["update-name"]) && !empty($_POST["update-genres"])) {
            update_movie($_SESSION['id'], $_POST["update-name"], $_POST["update-genres"]);
            header("Location: index.php");
    } else {
        echo "<script type='text/javascript'>alert('Invalid data');</script>";
        echo "<script type='text/javascript'>location.href='index.php';</script>";
    }

?>

<div class="container mt-5">
    <form action="updatemovie.php" method="post">
        <?php
            if (isset($_GET['id'])) {
                get_single_movie($id);
            }
        ?>
        <button type="submit" class="btn btn-primary">Submit</button>

    </form>
</div>

<?php
    include "footer.php";
?>
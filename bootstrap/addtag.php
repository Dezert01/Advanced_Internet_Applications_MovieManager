<?php
require_once("functions.php");
include("header.php");
session_start();
if (isset($_GET['id'])) {
    $id = $_GET['id'];
    $_SESSION['id'] = $id;
} else if (
    isset($_POST["form-user_id"], $_POST["form-tag"], $_SESSION['id'])
    and !empty($_POST["form-user_id"]) && !empty($_POST["form-tag"])
) {

    if (is_numeric($_POST["form-user_id"]) && check_if_user_exist($_POST["form-user_id"])) {
        if (check_if_have_tag($_POST["form-user_id"], $_SESSION['id'], $_POST["form-tag"])) {
            insert_tag($_POST["form-user_id"], $_SESSION['id'], $_POST["form-tag"]);
            header("Location: index.php");
            exit;
        } else {
            echo "<script type='text/javascript'>alert('Already added tag');</script>";
            echo "<script type='text/javascript'>location.href='index.php';</script>";
        }
    } else {
        echo "<script type='text/javascript'>alert('No User');</script>";
        echo "<script type='text/javascript'>location.href='index.php';</script>";
    }
} else {
    echo "<script type='text/javascript'>alert('Invalid data');</script>";
    echo "<script type='text/javascript'>location.href='index.php';</script>";
}

?>


<div class="container mt-5">
    <form action="addtag.php" method="post">
        <?php
        if (isset($_GET['id'])) {
            get_tag_add_page($id);
        }
        ?>
        <button type="submit" class="btn btn-primary">Submit</button>

    </form>
</div>

<?php
include "footer.php";
?>
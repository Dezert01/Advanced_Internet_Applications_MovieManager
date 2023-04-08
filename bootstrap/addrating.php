<?php 
    require_once ("functions.php");
    include ("header.php");
    session_start();
    if (isset($_GET['id'])) {
        $id = $_GET['id'];
        $_SESSION['id'] = $id;
    } else if (isset($_POST["form-user_id"], $_POST["form-rating"], $_SESSION['id'])

        and !empty($_POST["form-user_id"]) && !empty($_POST["form-rating"])) {
            if (is_numeric($_POST["form-rating"]) && $_POST["form-rating"]>=1 && $_POST["form-rating"]<=5) {
                
                if (is_numeric($_POST["form-user_id"]) && check_if_user_exist($_POST["form-user_id"])) {
                    if (check_if_have_rating($_POST["form-user_id"],$_SESSION['id']))
                    {
                        insert_rating($_POST["form-user_id"], $_SESSION['id'], intval($_POST["form-rating"]));
                        header("Location: index.php");
                        exit;

                    } else {
                        echo "<script type='text/javascript'>alert('Already added rating');</script>";
                        echo "<script type='text/javascript'>location.href='index.php';</script>";  
                    }
                  
                } else {
                    echo "<script type='text/javascript'>alert('No user');</script>";
                    echo "<script type='text/javascript'>location.href='index.php';</script>";  
                }
                    
            }else {
                echo "<script type='text/javascript'>alert('Invalid rating');</script>";
                echo "<script type='text/javascript'>location.href='index.php';</script>";  
            }
            
    } else {
        echo "<script type='text/javascript'>alert('Invalid data');</script>";
        echo "<script type='text/javascript'>location.href='index.php';</script>";
    }

?>


<div class="container mt-5">    
    <form action="addrating.php" method="post">
        <?php
            if (isset($_GET['id'])) {
                get_movie_rating_add_page($id);
            }
        ?>
        <button type="submit" class="btn btn-primary">Submit</button>

    </form>
</div>

<?php
    include "footer.php";
?>
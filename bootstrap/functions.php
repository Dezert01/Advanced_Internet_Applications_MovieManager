<?php
    require_once 'db_connection.php';

    function get_genres($genres) {
        $genres_array = explode('|', $genres);

        $html = '';

        foreach ($genres_array as $genre) {
            $html .= '<li class="list-group-item">' . $genre . '</li>';
        }

        return $html;
    }

    function get_genres_string($genres) {
        $genres_array = explode('|', $genres);

        $genres_string = implode(', ', $genres_array);

        return $genres_string;
    }

    function get_rating($id) {
        global $conn;
        $result = $conn -> query("SELECT AVG(rating) FROM ratings WHERE movieId = '$id' GROUP BY movieId");
        $row = mysqli_fetch_assoc($result);
        $result -> free_result();
        if (!is_null($row)){
            return round($row["AVG(rating)"],1);
        }
        return "Not rated";
    }

    function get_tags($id) {
        global $conn;
        $result = $conn -> query("SELECT userId, movieId, tag FROM tags WHERE movieId = '$id'");

        while ($row = mysqli_fetch_assoc($result)) {
            ?>
                <span class="badge rounded-pill badge-gray"><?php echo $row['tag'] ?><a class="badge-delete ml-1" role="button" href="deletetag.php?userId=<?php echo($row["userId"]) ?>&movieId=<?php echo($row["movieId"]) ?>&tag=<?php echo($row["tag"]) ?>">x</a></span>
            <?php
        }
        ?>
            <a class="badge rounded-pill badge-blue btn btn-primary" role="button" href="addtag.php?id=<?php echo $id ?>">+</a>
        <?php

        $result -> free_result();
        // return $html;
    }

    function get_all_data() {
        global $conn;
        if ($result = $conn -> query("SELECT * FROM Movies LIMIT 100")) {
            while ($row = mysqli_fetch_assoc($result)) {
                ?>
                    <div class="col-4">
                        <div class="card border-primary">
                            <div class="card-body">
                                <h4 class="card-title">
                                    <?php echo
                                        ($row["title"]) 
                                    ?>
                                </h4>
                                <h6 class="card-subtitle mb-2 text-muted">
                                    Rating: 
                                    <?php echo get_rating($row["movieId"])
                                    ?>
                                </h6>
                                <h6 class="card-subtitle mb-2 text-muted">
                                    Genres:
                                </h6>
                                <ul class="list-group list-group-flush">
                                    <?php echo 
                                        get_genres($row["genres"])
                                    ?>
                                </ul>
                                <a name="" id="" class="btn btn-primary" href="updatemovie.php?id=<?php echo($row["movieId"]) ?>" role="button">Update</a>
                                <a name="" id="" class="btn btn-success" href="addrating.php?id=<?php echo($row["movieId"]) ?>" role="button">Add rating</a>  
                                <a name="" id="" class="btn btn-danger" href="deletemovie.php?id=<?php echo($row["movieId"]) ?>" role="button">Delete</a>  
                                <a name="" id="" class="btn btn-secondary ml-auto" data-bs-toggle="collapse" href="#collapseTags-<?php echo $row["movieId"] ?>" role="button" aria-expanded="false" aria-controls="collapseTags-<?php echo $row["movieId"] ?>">Tags</a>
                                <div class="collapse mt-2" id="collapseTags-<?php echo $row["movieId"] ?>">
                                    <div class='d-flex gap-1 flex-wrap '>
                                        <?php echo get_tags($row["movieId"])?>
                                    </div>
                                </div> 
                            </div>
                        </div>
                    </div>
                <?php
            }
            $result -> free_result();
        }
    }

    function insert_new_movie($name, $genres) {
        global $conn;

        $genres_array = explode(', ', $genres);

        $genres_string = implode('|', $genres_array);

        $query = "INSERT INTO movies (title, genres) VALUES ('".$name."', '".$genres_string."')";
        if (!$conn -> query($query)) {
            die("Insertion error");
        }
    }

    function insert_new_user($name) {
        global $conn;

        $query = "INSERT INTO users (name) VALUES ('".$name."')";
        if (!$conn -> query($query)) {
            die("Insertion error");
        }
    }

    function delete_movie($id) {
        global $conn;
        $query = "DELETE FROM movies WHERE movieId = '".$id."'";
        if (!$conn -> query($query)) {
            die("Deletion error");
        }
    }

    function get_single_movie($id) {
        global $conn;
        if ($result = $conn -> query("SELECT * FROM movies WHERE movieId = '".$id."'")) {
            while ($row = mysqli_fetch_assoc($result)) {
                ?>
                    <div class="mb-3">
                        <label for="update-name" class="form-label">Movie Title</label>
                        <input type="text"
                            value="<?php echo ($row["title"]) ?>"
                            class="form-control" name="update-name" id="update-name" aria-describedby="nameId" placeholder="">
                        <small id="nameId" class="form-text text-muted">Title of the movie and year of realese in brackets (e.g. Star Wars: Episode VI - Return of the Jedi (1983))</small>
                    </div>
                    <div class="mb-3">
                        <label for="update-genres" class="form-label">Genres</label>
                        <input type="text"
                            value="<?php echo get_genres_string($row["genres"]) ?>"
                            class="form-control" name="update-genres" id="update-genres" aria-describedby="genresId" placeholder="">
                        <small id="genresId" class="form-text text-muted">Separate genres with commas (e.g. Action, Comedy, Drama)</small>
                    </div>
                <?php
            }
        }
        $result -> free_result();
    }

    function update_movie($id, $name, $genres) {
        global $conn;

        $genres_array = explode(', ', $genres);

        $genres_string = implode('|', $genres_array);
        $query = "UPDATE movies SET title = '".$name."', genres = '".$genres_string."' WHERE movieId = '".$id."'";
        if (!$conn -> query($query)) {
            die("Insertion error");
        }
    }

    function delete_tag($userId, $movieId, $tag) {
        global $conn;
        $query = "DELETE FROM tags WHERE userId = '".$userId."' and movieId = '".$movieId."' and tag = '".$tag."'";
        if (!$conn -> query($query)) {
            die("Deletion error");
        }
    }

    function check_if_user_exist($user_id) {
        global $conn;
        $result = $conn -> query("SELECT * FROM users WHERE id = '$user_id'");
        $row = mysqli_fetch_assoc($result);
        if (!is_null($row)){
            return true;
        }
        return false;
    }

    function check_if_have_rating($user_id, $movie_id) {
        global $conn;
        $result = $conn -> query("SELECT * FROM ratings WHERE userId = '$user_id' and movieId = '$movie_id'");
        $row = mysqli_fetch_assoc($result);
        if (!is_null($row)){
            return false;
        }
        return true;
    }

    function check_if_have_tag($user_id, $movie_id, $tag) {
        global $conn;
        $result = $conn -> query("SELECT * FROM tags  WHERE userId = '$user_id' and movieId = '$movie_id' and tag = '$tag'");
        $row = mysqli_fetch_assoc($result);
        if (!is_null($row)){
            return false;
        }
        return true;
    }
    
    function get_movie_rating_add_page($id) {
        global $conn;
        if ($result = $conn -> query("SELECT * FROM movies WHERE movieId = '".$id."'")) {
            while ($row = mysqli_fetch_assoc($result)) {
                ?>
                    <div class="mb-3">
                       <div> Movie Title: <?php echo ($row["title"]) ?></Title>
                    </div>
                    <div class="mb-3">
                        <label for="" class="form-label">Rating</label>
                        <input type="text" class="form-control" name="form-rating" id="form-rating" placeholder="">
                    </div>
                    <div class="mb-3">
                        <label for="" class="form-label">User ID</label>
                        <input type="text" class="form-control" name="form-user_id" id="form-user_id" placeholder="">
                    </div>
                <?php
            }
        }
        $result -> free_result();
    }

    function get_tag_add_page($id) {
        global $conn;
        if ($result = $conn -> query("SELECT * FROM movies WHERE movieId = '".$id."'")) {
            while ($row = mysqli_fetch_assoc($result)) {
                ?>
                    <div class="mb-3">
                       <div> Movie Title: <?php echo ($row["title"]) ?></Title>
                    </div>
                    <div class="mb-3">
                        <label for="" class="form-label">Tag</label>
                        <input type="text" class="form-control" name="form-tag" id="form-tag" placeholder="">
                    </div>
                    <div class="mb-3">
                        <label for="" class="form-label">User ID</label>
                        <input type="text" class="form-control" name="form-user_id" id="form-user_id" placeholder="">
                    </div>
                <?php
            }
        }
        $result -> free_result();
    }
    
    function insert_rating($user_id, $movie_id, $rating) {
        global $conn;
        $timestamp = time();

        $query = "INSERT INTO ratings (userId, movieId, rating, timestamp) VALUES ('$user_id', '$movie_id', '$rating', '$timestamp')";
        if (!$conn -> query($query)) {
            die("Rating insertion error");
        }
    }

    function insert_tag($user_id, $movie_id, $tag) {
        global $conn;
        $timestamp = time();

        $query = "INSERT INTO tags (userId, movieId, tag, timestamp) VALUES ('$user_id', '$movie_id', '$tag', '$timestamp')";
        if (!$conn -> query($query)) {
            die("Rating insertion error");
        }
    }
?>
<?php
    include("header.php");
    require_once "functions.php";
?>

<header>
    <ul class="nav nav-tabs" id="myTab" role="tablist">
        <li class="nav-item" role="presentation">
            <button class="nav-link active" id="movies-tab" data-bs-toggle="tab" data-bs-target="#movies" type="button" role="tab" aria-controls="movies" aria-selected="true">Movies</button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link" id="new-movie-tab" data-bs-toggle="tab" data-bs-target="#new-movie" type="button" role="tab" aria-controls="new-movie" aria-selected="false">Add New Movie</button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link" id="new-movie-tab" data-bs-toggle="tab" data-bs-target="#new-user" type="button" role="tab" aria-controls="new-user" aria-selected="false">Add New User</button>
        </li>
    </ul>
</header>

<main>
    <div class="tab-content">
        <div class="tab-pane active" id="movies" role="tabpanel" aria-labelledby="movies-tab"> 
            <div class="container">
                <div class="mb-3 mt-3">
                    <input type="text" class="form-control" placeholder="Search movies by title" aria-label="Search movies by title" id="searchBar" aria-describedby="searchBar">
                    <small id="searchBar" class="form-text text-muted">Press ENTER to display matching movies</small>
                </div>
                <div class="row justify-content-center align-items-start g-2" id="movieCards">
                    <?php
                        get_all_data();
                    ?>
                </div>
            </div>
        </div>
        <div class="tab-pane" id="new-movie" role="tabpanel" aria-labelledby="new-movie-tab">
            <div class="container mt-3">
                <form action="insertmovie.php" method="post">
                    <div class="mb-3">
                        <label for="name" class="form-label">Movie Name</label>
                        <input type="text"
                            class="form-control" name="name" id="name" aria-describedby="nameId" placeholder="" required>
                        <small id="nameId" class="form-text text-muted">Title of the movie and year of realese in brackets (e.g. Star Wars: Episode VI - Return of the Jedi (1983))</small>
                    </div>
                    <div class="mb-3">
                        <label for="genres" class="form-label">Genres</label>
                        <input type="text" 
                            class="form-control" id="genres" name="genres" aria-describedby="genresId" multiple required>
                        <small id="genresId" class="form-text text-muted">Separate genres with commas (e.g. Action, Comedy, Drama)</small>
                    </div>
                    <button type="submit" class="btn btn-primary">Submit</button>
                </form>
            </div>
        </div>
        <div class="tab-pane" id="new-user" role="tabpanel" aria-labelledby="new-user-tab">
            <div class="container mt-3">
                <form action="insertuser.php" method="post">
                    <div class="mb-3">
                        <label for="user-name" class="form-label">User Name</label>
                        <input type="text"
                            class="form-control" name="user-name" id="user-name" aria-describedby="user-nameId" placeholder="" required>
                        <small id="user-nameId" class="form-text text-muted">Name of the user</small>
                    </div>
                    <button type="submit" class="btn btn-primary">Submit</button>
                </form>
            </div>
        </div>
    </div> 
</main>

<?php include("footer.php") ?>
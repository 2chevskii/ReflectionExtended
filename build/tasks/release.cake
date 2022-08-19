#load ../data/build_data.cake

Task("release/create").Does<BuildData>(data => {


  /* GitReleaseManagerCreate(
    data.Environment.GitHubAccessToken,
    data.Environment.GitAuthorEmail,

  ); */
});

Task("release/publish").Does<BuildData>(data => {

});

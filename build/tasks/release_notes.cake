#load ../data/build_data.cake

Task("release-notes/collect").Does<BuildData>(data => {

});

Task("release-notes/write").IsDependentOn("release-notes/collect")
                           .Does<BuildData>(data => {

                           });

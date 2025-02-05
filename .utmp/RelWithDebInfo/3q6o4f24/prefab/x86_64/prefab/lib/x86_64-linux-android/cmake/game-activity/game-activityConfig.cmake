if(NOT TARGET game-activity::game-activity)
add_library(game-activity::game-activity STATIC IMPORTED)
set_target_properties(game-activity::game-activity PROPERTIES
    IMPORTED_LOCATION "C:/Users/Ariel/.gradle/caches/transforms-3/ce42ed48afc8684bea9950e972b212c2/transformed/jetified-games-activity-3.0.5/prefab/modules/game-activity/libs/android.x86_64/libgame-activity.a"
    INTERFACE_INCLUDE_DIRECTORIES "C:/Users/Ariel/.gradle/caches/transforms-3/ce42ed48afc8684bea9950e972b212c2/transformed/jetified-games-activity-3.0.5/prefab/modules/game-activity/include"
    INTERFACE_LINK_LIBRARIES ""
)
endif()

if(NOT TARGET game-activity::game-activity_static)
add_library(game-activity::game-activity_static STATIC IMPORTED)
set_target_properties(game-activity::game-activity_static PROPERTIES
    IMPORTED_LOCATION "C:/Users/Ariel/.gradle/caches/transforms-3/ce42ed48afc8684bea9950e972b212c2/transformed/jetified-games-activity-3.0.5/prefab/modules/game-activity_static/libs/android.x86_64/libgame-activity_static.a"
    INTERFACE_INCLUDE_DIRECTORIES "C:/Users/Ariel/.gradle/caches/transforms-3/ce42ed48afc8684bea9950e972b212c2/transformed/jetified-games-activity-3.0.5/prefab/modules/game-activity_static/include"
    INTERFACE_LINK_LIBRARIES ""
)
endif()


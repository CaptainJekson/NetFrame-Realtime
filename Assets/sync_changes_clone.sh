#!/bin/bash

# source path
source_folder_assets="/Users/evgeniyskvortsov/UnityProjects/NetFrame-Realtime/Assets"

# destination path
destination_folder_assets="/Users/evgeniyskvortsov/UnityProjects/NetFrame-Realtime_2/Assets"

# copy
/usr/bin/rsync -avz --exclude='.git*' --delete "$source_folder_assets/" "$destination_folder_assets/"
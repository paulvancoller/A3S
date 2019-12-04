#
# *************************************************
# Copyright (c) 2019, Grindrod Bank Limited
# License MIT: https://opensource.org/licenses/MIT
# **************************************************
#

REPO_USR=$1
REPO_PSW=$2
jar_name=$3
jar_location=$4
REPO_NAME=$5

errorExit () {
    echo -e "\nERROR: $1"; echo
    exit 1 
}


# Pushing the jar
pushJar() {
    echo -e "\nPushing Jar"
    echo "Jar: ${jar_name}"
    echo "jar_location: ${jar_location}"

	echo ""
	echo "-----------------------------"
	ls -la ${jar_location}
	echo "-----------------------------"
	echo ""
	
    [ ! -z "${jar_name}" ] || errorExit "Did not find the jar to deploy"
    status=$(curl -u${REPO_USR}:${REPO_PSW} -T ${jar_location}/${jar_name} -s -o /dev/null -w '%{http_code}' https://${REPO_NAME}/${jar_name})
    
    echo "curl -u${REPO_USR}:${REPO_PSW} -T ${jar_location}/${jar_name} https://${REPO_NAME}/${jar_name}"
    
    if [ 201 -ne $status ]; then
        # …(failure)
        errorExit "Uploading jar failed..  curl status response was $status"
    fi;
    
    echo
}

main () {
    echo -e "\nPushing the jar with following settings"

    echo "REPO_NAME:      ${REPO_NAME}"
    echo "REPO_USR:       ${REPO_USR}"
    echo "jar_name:       ${jar_name}"
    echo "jar_location:   ${jar_location}"    

    pushJar    
}

############## Main

main
pipeline {
    agent any
    stages {
        stage('List Plugins') {
            steps {
                script {
                    def plugins = Jenkins.get().pluginManager.plugins
                    plugins.each { plugin ->
                        echo "${plugin.getShortName()} (${plugin.getVersion()})"
                    }
                }
            }
        }
        stage("Checkout") {
			steps {
                script {
                    // checkout scmGit([
                    //     branches: [[name: "${env.BRANCH_NAME}"]],
                    //     extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: "${SOLUTION_DIR}"]],
                    //     userRemoteConfigs: [[credentialsId: 'Bitbucket', url: "http://192.168.2.183:7990/scm/pki/kms.git"]]
                    // ])

                    // def info = gitCheckout(url: 'pki/kms.git', branch: env.BRANCH_NAME, directory: "${SOLUTION_DIR}")

                    // env.LAST_COMMIT_HASH = "${info.GIT_COMMIT}"

                    //echo "Last Commit hash: ${env.LAST_COMMIT_HASH}"
                    echo "Git env: ${env.GIT_COMMIT}"
                    echo "Git commit: ${GIT_COMMIT}"
			    }
            }
        }
    }
	post {
		always {
			cleanWs()
		}
	}
}
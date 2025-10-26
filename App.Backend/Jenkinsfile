pipeline {
    agent any

    environment {
        FTP_HOST = 'ftp.morlido.com'
        FTP_USER = 'ftp_user_adin'
        FTP_PASS = 'ftp_sifren'
        FTP_DIR  = '/public_html/'
    }

    stages {
        stage('Checkout') {
            steps {
                echo 'Kod çekiliyor...'
                git branch: 'main', url: 'https://github.com/dgnyldrm7/FullStackRealtimeChatApplication.git'
            }
        }

        stage('Build') {
            steps {
                echo 'Proje derleniyor...'
                sh 'dotnet restore'
                sh 'dotnet build --configuration Release'
            }
        }

        stage('Publish') {
            steps {
                echo 'Release klasörü hazırlanıyor...'
                sh 'dotnet publish -c Release -o ./publish'
            }
        }

        stage('Deploy to FTP') {
            steps {
                echo 'FTP\'ye gönderiliyor...'
                ftpPublisher alwaysPublishFromMaster: true,
                continueOnError: false,
                publishers: [
                    [configName: 'default',
                     transfers: [[sourceFiles: 'publish/**', removePrefix: 'publish', remoteDirectory: "${FTP_DIR}"]],
                     usePromotionTimestamp: false, verbose: true]
                ]
            }
        }
    }
}

<!DOCTYPE html>
<html lang="en"> 
<body>
    <div class="container">
        <div class="header">
            <h1><i class="fas fa-rocket"></i> Installation Guide</h1>
            <p>Unity Web API Client - Quick Setup</p>
        </div> 
        <div class="content">
            <div class="method-tabs">
                <button class="tab active" onclick="showTab('recommended')">
                    <i class="fas fa-star"></i> Recommended
                </button>
                <button class="tab" onclick="showTab('manual')">
                    <i class="fas fa-code"></i> Manual
                </button>
                <button class="tab" onclick="showTab('advanced')">
                    <i class="fas fa-cogs"></i> Advanced
                </button>
            </div> 
            <!-- Recommended Method -->
            <div id="recommended" class="tab-content active">
                <div class="step">
                    <div class="step-header"> 
                        <div>
                            <h3>Install UniTask (Optional)</h3>
                            <small>Recommended for better performance</small>
                        </div>
                    </div>
                    <div class="step-content">
                        <p>Package Manager → + → Add package from git URL:</p>
                        <div class="code-block">
                            <button class="copy-btn" onclick="copy(this)">Copy</button>
                            <pre>https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask</pre>
                        </div>
                    </div>
                </div> 
                <div class="step">
                    <div class="step-header"> 
                        <div>
                            <h3>Install Web API Client</h3>
                            <small>Main package installation</small>
                        </div>
                    </div>
                    <div class="step-content">
                        <p>Same process - Package Manager → + → Add package from git URL:</p>
                        <div class="code-block">
                            <button class="copy-btn" onclick="copy(this)">Copy</button>
                            <pre>https://github.com/berkcankarabulut/UnityWebAPIClient.git</pre>
                        </div>
                        <div class="alert alert-success">
                            <strong><i class="fas fa-check"></i> Done!</strong> Both packages should appear in Package Manager.
                        </div>
                    </div>
                </div>
            </div> 
            <!-- Manual Method -->
            <div id="manual" class="tab-content">
                <div class="alert alert-warning">
                    <strong><i class="fas fa-exclamation-triangle"></i> Warning:</strong> Edit manifest.json carefully. Backup your project first.
                </div> 
                <h3>Edit Packages/manifest.json:</h3>
                <div class="code-block">
                    <button class="copy-btn" onclick="copy(this)">Copy</button>
                    <pre>{
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.berkcankarabulut.unity-web-api": "https://github.com/berkcankarabulut/UnityWebAPIClient.git"
  }
}</pre>
                </div>
                <p>Save file and Unity will auto-refresh packages.</p>
            </div> 
            <!-- Advanced Method -->
            <div id="advanced" class="tab-content">
                <h3>With OpenUPM Registry:</h3>
                <div class="code-block">
                    <button class="copy-btn" onclick="copy(this)">Copy</button>
                    <pre>{
  "dependencies": {
    "com.cysharp.unitask": "2.3.3",
    "com.berkcankarabulut.unity-web-api": "https://github.com/berkcankarabulut/UnityWebAPIClient.git"
  },
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": ["com.cysharp"]
    }
  ]
}</pre>
                </div>
            </div> 
            <!-- Troubleshooting -->
            <div class="troubleshooting">
                <h2><i class="fas fa-wrench"></i> Troubleshooting</h2> 
                <div class="problem">
                    <h4><i class="fas fa-times-circle"></i> "Package com.cysharp.unitask cannot be found"</h4>
                    <p><strong>Solution:</strong> Install UniTask first using the correct Git URL with ?path= parameter.</p>
                </div> 
                <div class="problem">
                    <h4><i class="fas fa-times-circle"></i> "Assembly references are missing"</h4>
                    <p><strong>Solution:</strong> Wait for Unity compilation to complete, then try Assets → Reimport All.</p>
                </div> 
                <div class="problem">
                    <h4><i class="fas fa-times-circle"></i> "Git URL not working"</h4>
                    <p><strong>Solution:</strong> Check internet connection and Git installation. Clear Unity's package cache.</p>
                </div>
            </div> 
            <!-- Verification -->
            <div class="verification">
                <h3><i class="fas fa-check-circle"></i> Verification</h3>
                <p>After installation, you should see:</p>
                <ul>
                    <li>Unity.WebAPI assembly in project</li>
                    <li>WebAPIManager component available</li>
                    <li>No compilation errors</li>
                    <li>Sample scripts accessible</li>
                </ul>
            </div> 
            <!-- Quick Start -->
            <div class="alert alert-info">
                <h4><i class="fas fa-rocket"></i> Quick Start</h4>
                <p>1. GameObject → Web API → Create Web API Manager<br>
                2. Configure API settings in inspector<br>
                3. Check samples for usage examples</p>
            </div>
        </div>
    </div>  
</body>
</html>

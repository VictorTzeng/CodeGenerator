app.codeGeneratorOption = {
    connectionString:'',
    outputPath:'D:\\CodeGenerates',
    solutionName:'',
    modelNamespace:'',
    viewmodelNamespace:'',
    controllerNamespace:'',
    iRepositoryNamespace:'',
    repositoryNamespace:'',
    iServiceNamespace:'',
    serviceNamespace:''
};
app.tableData = [];
app.loading = false;
app.connectDatabase = function(url) {
    var that = app;
    that.loading = true;
    that.$axios.get(url,
        {
            params: {
                connectionString: that.codeGeneratorOption.connectionString
            }
        }).then(function(result) {
        that.loading = false;
        if (result.data) {
            if (result.data.success) {
                that.tableData = result.data.rows;
                that.forceUpdateInput();
            } else {
                that.$message(result.data.msg);
            }
        }
    });
};
app.$watch(function() {
        return app.codeGeneratorOption.solutionName;
    },        
    function(newValue, oldValue) {
        app.codeGeneratorOption.modelNamespace = newValue + ".Models";
        app.codeGeneratorOption.viewmodelNamespace = newValue + ".ViewModels";
        app.codeGeneratorOption.controllerNamespace = newValue + ".Controllers";
        app.codeGeneratorOption.iRepositoryNamespace = newValue + ".IRepositories";
        app.codeGeneratorOption.repositoryNamespace = newValue + ".Repositories";
        app.codeGeneratorOption.iServiceNamespace = newValue + ".IServices";
        app.codeGeneratorOption.serviceNamespace = newValue + ".Service";
        app.forceUpdateInput();
    },
    {
        immediate: true,
        deep:true
    }
);

// Ignore this for now.. was up to date as of 3/30/2019 - it no longer is now!! Someday soon I'll commit to using TypeScript.

"use strict";

$(document).ready(function () {

    fetch("clientSettings.json")
    .then(response => { return response.json(); })
    .then( settings => { /* todo: something smart */ });

    var selectedInfoTables = ko.observableArray([]); // table(s) selected for query   

    //---------------------------------------------------------------------------------------------------sql-info
    var infoViewModel = undefined;
    selectRequired("#nav-info-tab").on("click", function (evt) {

        if (infoViewModel) return; // already been here? nothing to do?

        fetch("api/InfoSchemaTables")
        .then(response => { return response.json(); })
        .then(jsonResponse => {
            // ko viewmodel
            function InfoTablesViewModel( jsonData, queryTables) {

                var self = this,
                  allSchemas = [],
                  currentSchema = undefined,
                  isQueryBaseEstablished = false;

                // fixup json data with our UI needs
                jsonData.forEach(function (infoTable) {
                    infoTable.cardClass = ko.observable("");
                    infoTable.isSelectedQueryTable = ko.observable(false);
                    infoTable.isPrimaryQueryTable = ko.observable(false);
                    infoTable.isJoinedQueryTable = ko.observable(false);
                    infoTable.excludeFromQuery = ko.observable(false);
                    allSchemas.push(infoTable.table_schema);
                });
                self.infoTables =  jsonData;
                self.infoSchemas = [...new Set(allSchemas)]; // get unique schemas

                // methods
                self.setSchemaContext = function (schema) {
                    currentSchema = schema;
                    return currentSchema;
                };
                self.getTables = function () {
                    return self.infoTables.filter(function (item) {
                        return item.table_schema === currentSchema;
                    });
                };
                self.onTableClick = function (infoTable, event) {

                    if (isQueryBaseEstablished) {

                        infoTable.isPrimaryQueryTable(false);

                        var match = ko.utils.arrayFirst(queryTables(), function (item:any) {
                            return item.table_name === infoTable.table_name;
                        });
                        if (match) {
                            infoTable.isSelectedQueryTable(false);
                            infoTable.isJoinedQueryTable(false);
                            infoTable.cardClass("");
                            queryTables.pop(infoTable); // remove from query
                        } else {
                            infoTable.isSelectedQueryTable(true);
                            infoTable.isJoinedQueryTable(true);
                            infoTable.cardClass("flip-card-nolink");
                            queryTables.push(infoTable); // add to query
                        }

                        return;
                    }

                    queryTables([]); // start fresh, nothing selected
                    infoTable.isPrimaryQueryTable(true);
                    infoTable.isSelectedQueryTable(true);
                    infoTable.isJoinedQueryTable(false);
                    queryTables.push(infoTable);

                    // select card(s) related to current card
                    self.infoTables.forEach(function (infoTableRelated) {

                        var qualifiedTableName = infoTableRelated.table_schema + "." + infoTableRelated.table_name;

                        if (infoTable.fkList.indexOf(qualifiedTableName) > -1) {

                            infoTableRelated.isPrimaryQueryTable(false);
                            infoTableRelated.isSelectedQueryTable(true);
                            infoTableRelated.isJoinedQueryTable(true);
                            infoTableRelated.cardClass("flip-card-related");
                            queryTables.push(infoTableRelated);
                        } else {

                            infoTableRelated.cardClass("");
                        }
                    });

                    // select card that was clicked
                    infoTable.cardClass("flip-card-selected");
                };
                self.onLockClick = function (data) {

                    if (!isQueryBaseEstablished) {
                        self.onTableClick(data, null);
                        isQueryBaseEstablished = true;
                    }
                };
                self.onResetQueryClick = function (data) {

                    isQueryBaseEstablished = false;

                    queryTables([]); // starting over

                    self.infoTables.forEach(function (infoTable) {
                        infoTable.cardClass("");
                        infoTable.isPrimaryQueryTable(false);
                        infoTable.isSelectedQueryTable(false);
                        infoTable.isJoinedQueryTable(false);
                    });
                };
            }
            infoViewModel = new InfoTablesViewModel( jsonResponse, selectedInfoTables);
            ko.applyBindings(infoViewModel, document.getElementById("nav-info-partial") );
            })
        .catch(err => { console.log(err); alert("something went wroong.. see console log"); });

    });

    //---------------------------------------------------------------------------------------------------query-builder
    var queryViewModel = undefined;
    selectRequired("#nav-query-tab").on("click", function (evt) {

        if (selectedInfoTables().length === 0) {
            alert("pick a table to get started");
            return false;
        }
        if (queryViewModel) {

            queryViewModel.setQueryView( selectedInfoTables, true );

            queryViewModel.getQuerySql();
            queryViewModel.loadTablesData();

            $("table").resize(); // do not remove ... maintains column/header width

            return;
        } 

        // ko viewmodel
        function QueryViewModel( selectedInfoTables ) {

            var self = this;

            self.isNewMode = ko.observable(true);
            self.queryTables = ko.observableArray();
            self.sqlSelectStatement = ko.observable("");
            self.queryPk = undefined;

            self.queryName = ko.observable("");
            self.queryName.hasError = ko.observable(false);
            self.queryName.errorText = ko.observable("");
            self.queryName.subscribe(function (newValue) {
                self.isNewMode(true);
            });

            self.saveNamedQuery = function () {

                if (self.queryName() === "") {
                    self.queryName.hasError(true);
                    self.queryName.errorText("provide a unique query-name");

                    return;
                }

                var bodyData = { queryPk: self.queryPk, queryName: self.queryName(), querySql: self.sqlSelectStatement() };

                if (!self.isNewMode() ) {
                    fetch("/api/QueryRunner/UpdateNamedQuery", {
                        method: 'PUT',
                        headers: { "content-type": "application/json" },
                        body: JSON.stringify(bodyData)
                    })
                    .then(function (response) {

                        response.json().then(function (jsonData) {

                            if (response.status !== 200) {
                                self.queryName.hasError(true);
                                self.queryName.errorText(jsonData.appErrorText);
                            } else {
                                self.queryName.hasError(false);
                            }
                        });
                    })
                    .catch(function (err) { console.log(err); alert("something went wroong.. see console log"); });
                } else {
                    fetch("/api/QueryRunner/InsertNamedQuery", {
                        method: 'POST',
                        headers: { 'accept': 'application/json, text/plain, */*', "content-type": "application/json" },
                        body: JSON.stringify(bodyData)
                    })
                    .then(function (response) {

                        response.json().then(function (jsonData) {

                            if (response.status !== 200) {
                                self.queryName.hasError(true);
                                self.queryName.errorText(jsonData.appErrorText);
                            } else {
                                self.queryName.hasError(false);
                                self.queryPk = jsonData.result;
                            }
                            self.isNewMode(self.queryName.hasError());
                        });
                    })
                    .catch(function (err) { console.log(err); alert("something went wroong.. see console log"); });
                }
            };
            self.getQuerySql = function() {

                var bodyData = [];
                ko.utils.arrayForEach(self.queryTables(), function (qt:any) {
                    if (qt.excludeFromQuery() === false) {
                        var columnsArray = qt.columnList.filter((el) => !qt.columnListX.includes(el)); // isolate the desired column(s)
                        bodyData.push({ JoinOn: qt.queryContitions(), IsBaseTable: qt.isPrimaryQueryTable(), TableName: qt.querySchemaPlusTable, PkColumnName: qt.pkName, IncludeColumns: columnsArray, FkTableNames: qt.fkList });
                    }
                });

                fetch("/api/queryrunner/MakeSqlQueryString", {
                    method: 'POST',
                    headers: { "content-type": "application/json" },
                    body: JSON.stringify(bodyData)
                })
                .then(response => { return response.json(); })
                .then(data => {
                    self.sqlSelectStatement(data.fullSql);
                });
            };
            self.getQueryResult = function () {

                var bodyData = { sqlQueryStatement: self.sqlSelectStatement() };

                fetch("/api/QueryRunner/RunSqlQuery", {
                        method: 'PUT',
                        headers: { "content-type": "application/json" },
                        body: JSON.stringify(bodyData)
                    })
                    .then(function (response) {

                        response.json().then(function (jsonData) { console.log(jsonData); });
                    })
                    .catch(function (err) { console.log(err); alert("something went wroong.. see console log"); });
            };
            self.setQueryView = function (selectedTables, isReset) {

                self.queryTables([]);

                ko.utils.arrayForEach(selectedTables(), function (queryTable:any) {

                    queryTable.queryTable = queryTable.table_name;
                    queryTable.querySchemaPlusTable = queryTable.table_schema + "." + queryTable.table_name;
                    queryTable.queryId = queryTable.table_name;
                    queryTable.queryTitle = queryTable.querySchemaPlusTable + " " + (queryTable.isPrimaryQueryTable() ? "(primary)" : "(include/join)");
                    queryTable.queryContitions = ko.observable(""); //todo: something better than this

                    self.queryTables.push(queryTable);
                });
            };
            self.loadTablesData = function () {

                ko.utils.arrayForEach( self.queryTables(), function ( queryTable:any ) {

                    var dtColumns = [];

                    queryTable.columnList.forEach(function (columnName) {
                        dtColumns.push({ data: columnName, title: columnName, class: "cell-nowrap" });
                    });

                    var dt:any = $("table#" + queryTable.queryId).DataTable({
                        destroy: true, paging: false, searching: false, info: false, ordering: false, scrollX: true, autoWidth: true,
                        ajax: {
                            url: "/api/queryrunner/" + queryTable.querySchemaPlusTable,
                            data: { isPrimary: queryTable.isPrimaryQueryTable() },
                            dataSrc: ""
                        },
                        columns: dtColumns
                    });

                    let headers = dt.columns().header().toArray();
                    $(headers).on('click', function (tableHeader) {
                        var index = dt.column(this).index();
                        var clickedColumn = dtColumns[index];
                        queryTable.columnListX.push(clickedColumn.data);
                        dt.column(dt.column(this).index()).visible(false);
                        dt.draw();
                    });

                    dt.on("click", "td", function (row:any) {
                        var cellData = dt.cell(this).data();
                        var cellX = dt.cell(this).index().column;
                        var cellName = headers[cellX].innerHTML;

                        var foo = queryTable.queryContitions();
                        if (foo === "") {
                            queryTable.queryContitions(cellName + " = '" + cellData + "'");
                        } else {
                            queryTable.queryContitions(foo + " | " + cellName + " = '" + cellData + "'");
                        }
                    });

                });
            };

            self.setQueryView(selectedInfoTables, false); //
        }
        queryViewModel = new QueryViewModel( selectedInfoTables );
        ko.applyBindings( queryViewModel, document.getElementById("nav-query-partial"));

        queryViewModel.getQuerySql();
        queryViewModel.loadTablesData();

        $("table").resize(); // do not remove ... maintains column/header width
    });

    //---------------------------------------------------------------------------------------------------get-the-code
    var codeViewModel = undefined;
    selectRequired("#nav-code-tab").on("click", function (evt) {

        if (!queryViewModel || queryViewModel.isNewMode()) {
            alert("name and save your query before viewing code");
            return false;
        }

        // ko viewmodel
        function CodeViewModel() {

            var self = this;

            self.queryName = ko.observable(queryViewModel.queryName());

            // bound methods
            self.runQuery = function() {

                fetch("/api/foobars?queryName=" + self.queryName(), { method: 'GET' })
                .then(response => { return response.json(); })
                .then(jsonData => { console.log(jsonData); 
                });
            };
        }
        codeViewModel = new CodeViewModel();
        ko.applyBindings(codeViewModel, document.getElementById("nav-code-partial"));

    });

    
    //---------------------------------------------------------------------------------------------------start here
    selectRequired("#nav-info-tab").trigger('click'); // <<<<<<<<<<<<< get started like this Dave

    
    
    //---------------------------------------------------------------------------------------------------usefull stuff
    function selectRequired(selector) {

        var wrappedSet = $(selector);

        if (wrappedSet.length === 0) {
            alert("programming issue.. required selector returned zero");
        }

        return $(wrappedSet);
    }
});
